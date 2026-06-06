using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using Group = UnityEditor.Experimental.GraphView.Group;

namespace ALTUSHKA.TheAtlas
{
    // ==================================================================
    // MAIN WINDOW
    // ==================================================================
    public class ClassDependencyGraph : EditorWindow
    {
        private string ToHex(Color c) => $"#{ColorUtility.ToHtmlStringRGB(c)}";

        private DependencyGraphView _graphView;
        private DefaultAsset _selectedFolder;
        private GraphLayoutPreset _currentPreset;
        private ToolbarSearchField _searchField;
        private ObjectField _presetField;
        private ObjectField _folderField;
        private bool _showInheritedMembers = true;

        [MenuItem("Window/The Atlas/Dependency Graph")]
        public static void OpenWindow()
        {
            var window = GetWindow<ClassDependencyGraph>();
            window.titleContent = new GUIContent("The Atlas");
            window.Show();
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
        }

        private void OnDisable()
        {
            if (_graphView != null) rootVisualElement.Remove(_graphView);
        }

        public void TriggerRefreshFromView()
        {
            GenerateGraph(true);
        }

        private void ConstructGraphView()
        {
            var oldGraph = rootVisualElement.Q<DependencyGraphView>();
            if (oldGraph != null) rootVisualElement.Remove(oldGraph);

            _graphView = new DependencyGraphView(this) { name = "TheAtlasView" };
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void GenerateToolbar()
        {
            var existingToolbar = rootVisualElement.Q<Toolbar>();
            if (existingToolbar != null) rootVisualElement.Remove(existingToolbar);

            var toolbar = new Toolbar { style = { height = 30, paddingTop = 2, paddingLeft = 5, paddingRight = 5 } };

            _folderField = new ObjectField
            {
                objectType = typeof(DefaultAsset),
                allowSceneObjects = false,
                value = _selectedFolder,
                style = { width = 200, marginRight = 5 },
                tooltip = "Folder for analysis (leave empty for Assets)"
            };
            _folderField.RegisterValueChangedCallback(evt =>
            {
                _selectedFolder = evt.newValue as DefaultAsset;
                GenerateGraph(false);
            });

            var refreshButton = new ToolbarButton(() => GenerateGraph(true))
            {
                text = "Refresh",
                tooltip = "Rescan code and rebuild graph",
                style = { width = 60, marginRight = 10, unityTextAlign = TextAnchor.MiddleCenter }
            };

            _searchField = new ToolbarSearchField { style = { width = 200, marginRight = 10 } };
            _searchField.RegisterValueChangedCallback(evt => _graphView.ApplySearch(evt.newValue));

            var focusToggle = new ToolbarToggle
            {
                text = "Focus Mode",
                value = true,
                tooltip = "Dim other nodes when one is selected",
                style = { marginRight = 10 }
            };
            focusToggle.RegisterValueChangedCallback(evt =>
            {
                _graphView.FocusModeEnabled = evt.newValue;
                _graphView.RefreshFocusMode();
            });

            var inheritedToggle = new ToolbarToggle
            {
                text = "Show Inherited Members",
                value = _showInheritedMembers,
                tooltip = "Hide fields, properties, and interfaces inherited from base classes",
                style = { marginRight = 10 }
            };
            inheritedToggle.RegisterValueChangedCallback(evt =>
            {
                _showInheritedMembers = evt.newValue;
                GenerateGraph(true);
            });


            toolbar.Add(_folderField);
            toolbar.Add(refreshButton);
            toolbar.Add(_searchField);
            toolbar.Add(focusToggle);
            toolbar.Add(inheritedToggle);

            toolbar.Add(new ToolbarSpacer { style = { flexGrow = 1 } });

            _presetField = new ObjectField
            {
                objectType = typeof(GraphLayoutPreset),
                allowSceneObjects = false,
                value = _currentPreset,
                style = { width = 180, marginRight = 5 }
            };
            _presetField.RegisterValueChangedCallback(evt =>
            {
                _currentPreset = evt.newValue as GraphLayoutPreset;
                if (_currentPreset != null) LoadFromPreset();
            });

            var saveButton = new ToolbarButton(SaveToPreset) { text = "Save", tooltip = "Save current layout to preset", style = { width = 50 } };
            var loadButton = new ToolbarButton(LoadFromPreset) { text = "Load", tooltip = "Load layout from selected preset", style = { width = 50 } };

            toolbar.Add(_presetField);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);

            rootVisualElement.Add(toolbar);
        }

        // --- PRESETS ---
        private void SaveToPreset()
        {

            bool isPresetMissing = _currentPreset == null;
            if (!isPresetMissing && EditorUtility.IsPersistent(_currentPreset) && !AssetDatabase.Contains(_currentPreset))
                isPresetMissing = true;


            if (isPresetMissing)
            {
                string path = EditorUtility.SaveFilePanelInProject("Save Layout Preset", "NewGraphPreset", "asset", "Save Layout");
                if (string.IsNullOrEmpty(path)) return;

                var newPreset = ScriptableObject.CreateInstance<GraphLayoutPreset>();
                AssetDatabase.CreateAsset(newPreset, path);
                AssetDatabase.SaveAssets();

                _currentPreset = newPreset;
                if (_presetField != null) _presetField.value = _currentPreset;
            }

            var positions = new Dictionary<string, Vector2>();
            var boxes = new List<BoxData>();
            var groups = new List<GroupData>();

            foreach (var element in _graphView.graphElements)
            {
                if (element is TypeNode typeNode)
                {
                    positions[typeNode.FullTypeName] = typeNode.GetPosition().position;
                }
                else if (element is BoxNode boxNode)
                {
                    boxes.Add(new BoxData
                    {
                        Title = boxNode.title,
                        Position = boxNode.GetPosition().position,
                        ContainedTypes = boxNode.GetContainedTypeNames()
                    });
                    foreach (var hidden in boxNode.StoredNodes)
                        positions[hidden.FullTypeName] = hidden.CachedPosition.position;
                }
                else if (element is Group group)
                {
                    var nodeIds = new List<string>();
                    foreach (var child in group.containedElements)
                        if (child is TypeNode tn) nodeIds.Add(tn.FullTypeName);

                    groups.Add(new GroupData
                    {
                        Title = group.title,
                        Position = group.GetPosition().position,
                        ContainedNodeIds = nodeIds
                    });
                }
            }

            if (_currentPreset != null)
            {
                _currentPreset.UpdateData(positions, boxes, groups, _selectedFolder);
                EditorUtility.SetDirty(_currentPreset);
                AssetDatabase.SaveAssets();
            }
        }

        private void LoadFromPreset()
        {
            if (_currentPreset != null)
            {
                if (_currentPreset.LinkedFolder != null)
                {
                    _selectedFolder = _currentPreset.LinkedFolder;
                    if (_folderField != null) _folderField.SetValueWithoutNotify(_selectedFolder);
                }
                GenerateGraph(false, _currentPreset.GetPositions(), _currentPreset.Boxes, _currentPreset.Groups);
            }
        }

        // --- GRAPH GENERATION --- сейв
        private void GenerateGraph(bool preservePositions, Dictionary<string, Vector2> forcedPositions = null, List<BoxData> boxesToRestore = null, List<GroupData> groupsToRestore = null)
        {
            Dictionary<string, Vector2> positions = forcedPositions ?? new Dictionary<string, Vector2>();
            List<BoxData> currentBoxes = boxesToRestore;
            List<GroupData> currentGroups = groupsToRestore;

            if (preservePositions && forcedPositions == null && currentBoxes == null)
            {
                currentBoxes = new List<BoxData>();
                currentGroups = new List<GroupData>();
                foreach (var el in _graphView.graphElements)
                {
                    if (el is Group g)
                    {
                        var ids = new List<string>();
                        foreach (var child in g.containedElements) if (child is TypeNode tn) ids.Add(tn.FullTypeName);
                        currentGroups.Add(new GroupData { Title = g.title, Position = g.GetPosition().position, ContainedNodeIds = ids });
                    }
                }
                foreach (var node in _graphView.nodes)
                {
                    if (node is TypeNode typeNode) positions[typeNode.FullTypeName] = typeNode.GetPosition().position;
                    else if (node is BoxNode boxNode)
                    {
                        currentBoxes.Add(new BoxData { Title = boxNode.title, Position = boxNode.GetPosition().position, ContainedTypes = boxNode.GetContainedTypeNames() });
                        foreach (var hidden in boxNode.StoredNodes) positions[hidden.FullTypeName] = hidden.CachedPosition.position;
                    }
                }
            }

            _graphView.DeleteElements(_graphView.graphElements.ToList());

            string filterPath = "Assets";
            if (_selectedFolder != null)
            {
                filterPath = AssetDatabase.GetAssetPath(_selectedFolder);
                if (!AssetDatabase.IsValidFolder(filterPath)) return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Script", new[] { filterPath });
            var fileContents = new Dictionary<string, string>();
            var typeToPath = new Dictionary<Type, string>();

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".cs")) fileContents[path] = File.ReadAllText(path);
            }

            if (fileContents.Count == 0) return;

            var allTypes = Assembly.Load("Assembly-CSharp").GetTypes();

            var relevantTypes = new List<Type>();
            var typeContexts = new Dictionary<Type, (string codeBody, List<string> usings)>();
            var typeLines = new Dictionary<Type, int>();

            foreach (var type in allTypes)
            {
                if (type.FullName.Contains("Editor") || type.Name.StartsWith("<")) continue;
                foreach (var kvp in fileContents)
                {
                    string content = kvp.Value;
                    if (!content.Contains(type.Name)) continue;

                    string pattern = $@"\b(class|interface|enum|struct)\s+{type.Name}\b";
                    Match match = Regex.Match(content, pattern);

                    if (match.Success)
                    {
                        string isolatedCode = ExtractTypeBlock(content, type.Name);
                        if (!string.IsNullOrEmpty(isolatedCode))
                        {
                            relevantTypes.Add(type);
                            typeToPath[type] = kvp.Key;

                            int lineNum = GetLineFromIndex(content, match.Index);
                            typeLines[type] = lineNum;

                            var usings = new List<string>();
                            foreach (Match m in Regex.Matches(content, @"\busing\s+static\s+([\w\.]+);")) usings.Add(m.Groups[1].Value);
                            typeContexts[type] = (isolatedCode, usings);
                            break;
                        }
                    }
                }
            }

            var allTypeNodes = new Dictionary<Type, TypeNode>();
            int x = 0, y = 0;

            foreach (var type in relevantTypes.OrderBy(t => t.Name))
            {
                int line = typeLines.ContainsKey(type) ? typeLines[type] : 1;
                var node = CreateBasicNode(type, typeToPath.GetValueOrDefault(type, ""), line);

                if (positions.TryGetValue(type.FullName, out Vector2 pos)) { Rect r = new Rect(pos, Vector2.zero); node.SetPosition(r); node.CachedPosition = r; }
                else { Rect r = new Rect(x, y, 0, 0); node.SetPosition(r); node.CachedPosition = r; x += 350; if (x > 1400) { x = 0; y += 300; } }
                allTypeNodes[type] = node;
            }

            var typeToBoxMap = new Dictionary<TypeNode, BoxNode>();
            var boxedTypes = new HashSet<TypeNode>();
            var boxNodesToAdd = new List<BoxNode>();

            if (currentBoxes != null)
            {
                foreach (var boxData in currentBoxes)
                {
                    var boxNode = new BoxNode(boxData.Title);
                    boxNode.SetPosition(new Rect(boxData.Position, Vector2.zero));
                    var nodesInThisBox = new List<TypeNode>();
                    foreach (var typeName in boxData.ContainedTypes)
                    {
                        var node = allTypeNodes.Values.FirstOrDefault(n => n.FullTypeName == typeName);
                        if (node != null) { nodesInThisBox.Add(node); boxedTypes.Add(node); typeToBoxMap[node] = boxNode; }
                    }
                    if (nodesInThisBox.Count > 0) { boxNode.StoredNodes = nodesInThisBox; boxNode.RefreshContentList(); boxNodesToAdd.Add(boxNode); }
                }
            }

            foreach (var kvp in allTypeNodes) { if (!boxedTypes.Contains(kvp.Value)) _graphView.AddElement(kvp.Value); }
            foreach (var box in boxNodesToAdd) _graphView.AddElement(box);

            if (currentGroups != null)
            {
                foreach (var gData in currentGroups)
                {
                    _graphView.CreateGroupFromSelection(gData.Title, gData.Position, null);
                    var group = _graphView.graphElements.OfType<Group>().Last();
                    foreach (var id in gData.ContainedNodeIds)
                    {
                        var node = allTypeNodes.Values.FirstOrDefault(n => n.FullTypeName == id);
                        if (node != null && !boxedTypes.Contains(node)) group.AddElement(node);
                    }
                }
            }

            foreach (var type in relevantTypes)
            {
                if (!allTypeNodes.ContainsKey(type)) continue;
                var sourceNode = allTypeNodes[type];
                var rawDependencies = GetDependencies(type, relevantTypes, typeContexts[type].codeBody, typeContexts[type].usings, typeLines[type], typeToPath, fileContents);

                foreach (var group in rawDependencies.GroupBy(d => d.TargetType))
                {
                    if (!allTypeNodes.ContainsKey(group.Key)) continue;
                    var targetNode = allTypeNodes[group.Key];

                    BoxNode sourceBox = typeToBoxMap.ContainsKey(sourceNode) ? typeToBoxMap[sourceNode] : null;
                    BoxNode targetBox = typeToBoxMap.ContainsKey(targetNode) ? typeToBoxMap[targetNode] : null;

                    if (sourceBox != null && sourceBox == targetBox) continue;

                    var dominantRelation = group.OrderBy(d => GetPriority(d.RelationType)).First().RelationType;
                    Color blockColor = GetColorForDependency(dominantRelation);

                    var dependencyLines = group.GroupBy(x => x.Description).Select(mg =>
                    {
                        var distinctTypes = mg.Select(x => x.RelationType).Distinct().OrderBy(t => GetPriority(t)).ToList();
                        string lineBuilder = "";

                        foreach (var t in distinctTypes)
                        {
                            string tagHex = ToHex(GetColorForDependency(t));
                            lineBuilder += $"<color={tagHex}>[{GetShortRelation(t)}]</color>";
                        }

                        string nameHex = ToHex(GetColorForDependency(distinctTypes.First()));
                        lineBuilder += $" <color={nameHex}>{mg.Key}</color>";

                        return new DependencyLineData
                        {
                            DisplayText = lineBuilder,
                            FilePath = mg.First().SourcePath,
                            LineNumber = mg.First().SourceLine
                        };
                    }).ToList();

                    Port outPort;
                    Port inPort;

                    if (sourceBox != null)
                        outPort = sourceBox.GetOrCreatePort(Direction.Output, $"{type.Name} > {targetNode.title}", blockColor);
                    else
                        outPort = sourceNode.AddDependencyPort(targetBox != null ? (GraphElement)targetBox : targetNode, dependencyLines, blockColor);

                    if (targetBox != null)
                        inPort = targetBox.GetOrCreatePort(Direction.Input, $"{targetNode.title} (In)", Color.white);
                    else
                        inPort = targetNode.MainInputPort;

                    if (outPort != null && inPort != null)
                    {
                        var edge = new Edge { output = outPort, input = inPort };

                        _graphView.AddElement(edge);
                        edge.input.Connect(edge);
                        edge.output.Connect(edge);

                        edge.output.portColor = edge.input.portColor = blockColor;
                    }
                }
            }

            foreach (var node in allTypeNodes.Values) node.UpdateSmartRouting();
            _graphView.RefreshFocusMode();
        }

        private Port GetOrCreateBoxPort(BoxNode box, Direction direction, string portName, Color color)
        {
            var container = direction == Direction.Output ? box.outputContainer : box.inputContainer;

            foreach (var child in container.Children())
            {
                if (child is Port p && p.portName == portName) return p;
            }

            Port.Capacity capacity = Port.Capacity.Multi;
            var port = Port.Create<Edge>(Orientation.Horizontal, direction, capacity, typeof(bool));
            port.portName = portName;
            port.portColor = color;

            var labelEl = port.Q<Label>();

            if (labelEl != null)
            {
                labelEl.style.fontSize = 11;
                labelEl.style.color = new StyleColor(Color.Lerp(color, Color.white, 0.5f));
            }

            container.Add(port);
            box.RefreshPorts();
            return port;
        }

        private string ExtractTypeBlock(string fileContent, string typeName)
        {
            string pattern = $@"\b(class|interface|enum|struct)\s+{typeName}\b";
            Match match = Regex.Match(fileContent, pattern);
            if (!match.Success) return "";

            int openBraceIndex = fileContent.IndexOf('{', match.Index);
            if (openBraceIndex == -1) return "";

            int depth = 0;
            int index = openBraceIndex;
            bool insideString = false;
            bool insideChar = false;
            bool insideLineComment = false;
            bool insideBlockComment = false;

            while (index < fileContent.Length)
            {
                char c = fileContent[index];
                char next = index + 1 < fileContent.Length ? fileContent[index + 1] : '\0';

                if (!insideString && !insideChar && !insideBlockComment && !insideLineComment)
                {
                    if (c == '/' && next == '/') { insideLineComment = true; index++; continue; }
                    if (c == '/' && next == '*') { insideBlockComment = true; index++; continue; }
                }
                if (insideLineComment && c == '\n') { insideLineComment = false; }
                if (insideBlockComment && c == '*' && next == '/') { insideBlockComment = false; index++; continue; }

                if (!insideLineComment && !insideBlockComment)
                {
                    if (c == '"' && !insideChar && (index == 0 || fileContent[index - 1] != '\\')) insideString = !insideString;
                    else if (c == '\'' && !insideString && (index == 0 || fileContent[index - 1] != '\\')) insideChar = !insideChar;

                    if (!insideString && !insideChar)
                    {
                        if (c == '{') depth++;
                        else if (c == '}')
                        {
                            depth--;
                            if (depth == 0)
                                return fileContent.Substring(match.Index, index - match.Index + 1);
                        }
                    }
                }
                index++;
            }
            return "";
        }

        private int GetLineFromIndex(string text, int index)
        {
            if (string.IsNullOrEmpty(text) || index < 0 || index > text.Length) return 1;
            int line = 1;
            for (int i = 0; i < index; i++)
            {
                if (text[i] == '\n') line++;
            }
            return line;
        }

        private int FindMethodDefinitionLine(string content, string methodName, int defaultLine = 1)
        {
            if (string.IsNullOrEmpty(content)) return defaultLine;

            Match m = Regex.Match(content, $@"\b{Regex.Escape(methodName)}\b\s*(\(|{{|\=|=>)");

            if (m.Success)
            {
                return GetLineFromIndex(content, m.Index);
            }
            return defaultLine;
        }

        // =========================================================
        // SETTINGS AND LOGIC
        // =========================================================

        public enum RelationType { Inheritance, Interface, PropGet, PropSet, Field, Enum, MethodCall, EventSub, Instance, StaticProp, GetComponent, Debug }

        public struct Dependency
        {
            public Type TargetType;
            public RelationType RelationType;
            public string Description;
            public string SourcePath; 
            public int SourceLine;
        }

        private string GetShortRelation(RelationType type)
        {
            return type switch
            {
                RelationType.Inheritance => "Base",
                RelationType.Interface => "Impl",
                RelationType.MethodCall => "Call",
                RelationType.EventSub => "EventSub",
                RelationType.Field => "Field",
                RelationType.PropGet => "Get",
                RelationType.PropSet => "Set",
                RelationType.Instance => "Inst",
                RelationType.StaticProp => "Static",
                RelationType.GetComponent => "GetComp",
                RelationType.Debug => "Debug",
                RelationType.Enum => "Enum",
                _ => "?"
            };
        }

        private Color GetColorForDependency(RelationType type)
        {
            return type switch
            {
                RelationType.Inheritance => new Color(0.3f, 0.6f, 1f),
                RelationType.Interface => new Color(0.2f, 0.8f, 0.9f),
                RelationType.MethodCall => new Color(1f, 0.8f, 0.2f),
                RelationType.EventSub => new Color(1f, 0.4f, 0.4f),
                RelationType.Field => new Color(0.4f, 0.8f, 0.4f),
                RelationType.Instance => new Color(0.2f, 0.9f, 0.6f),
                RelationType.StaticProp => new Color(0.7f, 0.7f, 0.5f),
                RelationType.PropGet => new Color(1f, 0.5f, 0.8f),
                RelationType.PropSet => new Color(1f, 0.6f, 0.0f),
                RelationType.GetComponent => new Color(0.3f, 0.6f, 0.9f),
                RelationType.Debug => new Color(0.5f, 0.5f, 0.5f),
                RelationType.Enum => new Color(0.7f, 0.6f, 0.9f),
                _ => Color.white
            };
        }

        private int GetPriority(RelationType type)
        {
            return type switch
            {
                RelationType.Debug => -1,
                RelationType.Inheritance => 0,
                RelationType.Interface => 1,
                RelationType.MethodCall => 2,
                RelationType.EventSub => 3,
                RelationType.PropSet => 4,
                RelationType.PropGet => 5,
                RelationType.Field => 6,
                RelationType.Enum => 7,
                RelationType.StaticProp => 8,
                RelationType.Instance => 9,
                RelationType.GetComponent => 10,
                _ => 12
            };
        }

        private List<Dependency> GetDependencies(Type type, List<Type> scope, string codeBody, List<string> rawUsings, int classStartLine, Dictionary<Type, string> typeToPath, Dictionary<string, string> fileContents)
        {
            var list = new List<Dependency>();
            string currentFilePath = typeToPath.ContainsKey(type) ? typeToPath[type] : "";

            string c = RemoveComments(codeBody);

            if (type.BaseType != null && scope.Contains(type.BaseType))
                list.Add(new Dependency { TargetType = type.BaseType, RelationType = RelationType.Inheritance, Description = "base", SourceLine = classStartLine, SourcePath = currentFilePath });

            var allInterfaces = type.GetInterfaces();
            var baseInterfaces = type.BaseType != null ? type.BaseType.GetInterfaces() : Array.Empty<Type>();
            foreach (var i in allInterfaces)
            {
                if (scope.Contains(i))
                {
                    if (!_showInheritedMembers && baseInterfaces.Contains(i)) continue;

                    try
                    {
                        var map = type.GetInterfaceMap(i);

                        for (int k = 0; k < map.InterfaceMethods.Length; k++)
                        {
                            var interfaceMethod = map.InterfaceMethods[k];
                            var implementationMethod = map.TargetMethods[k];

                            string descriptionName = interfaceMethod.Name.Replace("get_", "").Replace("set_", "");

                            Type implType = implementationMethod.DeclaringType;
                            string targetPath = currentFilePath;
                            int targetLine = classStartLine;

                            if (implType != type && typeToPath.ContainsKey(implType))
                            {
                                targetPath = typeToPath[implType];
                                string parentContent = fileContents[targetPath];
                                string searchName = implementationMethod.Name;

                                if (searchName.StartsWith("get_") || searchName.StartsWith("set_")) searchName = searchName.Substring(4);

                                targetLine = FindMethodDefinitionLine(parentContent, searchName, 1);
                            }
                            else
                            {
                                string searchName = implementationMethod.Name;
                                if (searchName.StartsWith("get_") || searchName.StartsWith("set_"))
                                    searchName = searchName.Substring(4);

                                int localLine = FindMethodDefinitionLine(codeBody, searchName, 0);
                                if (localLine > 0) targetLine = classStartLine + localLine - 1;
                            }

                            if (!list.Any(d => d.TargetType == i && d.Description == descriptionName))
                            {
                                list.Add(new Dependency
                                {
                                    TargetType = i,
                                    RelationType = RelationType.Interface,
                                    Description = descriptionName,
                                    SourcePath = targetPath,
                                    SourceLine = targetLine
                                });
                            }
                        }
                    }
                    catch
                    {
                        list.Add(new Dependency { TargetType = i, RelationType = RelationType.Interface, Description = "implements", SourceLine = classStartLine, SourcePath = currentFilePath });
                    }
                }
            }

            var variableMap = new Dictionary<string, Type>();

            var fieldFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            if (!_showInheritedMembers) fieldFlags |= BindingFlags.DeclaredOnly; 

            foreach (var f in type.GetFields(fieldFlags))
            {
                variableMap[f.Name] = f.FieldType;
                Type depType = f.FieldType;
                if (depType.IsGenericType && depType.GetGenericArguments().Length > 0) depType = depType.GetGenericArguments()[0];
                else if (depType.IsArray) depType = depType.GetElementType();

                if (depType != null && scope.Contains(depType) && depType != type)
                {
                    var rel = depType.IsEnum ? RelationType.Enum : RelationType.Field;
                    string desc = f.FieldType.IsGenericType ? $"List<{f.Name}>" : f.Name;

                    int fieldLine = classStartLine;
                    Match m = Regex.Match(c, $@"\b{Regex.Escape(f.Name)}\b");
                    if (m.Success) fieldLine = classStartLine + GetLineFromIndex(c, m.Index) - 1;

                    list.Add(new Dependency { TargetType = depType, RelationType = rel, Description = desc, SourceLine = fieldLine, SourcePath = currentFilePath });
                }
            }

            var propFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            if (!_showInheritedMembers) propFlags |= BindingFlags.DeclaredOnly;

            foreach (var p in type.GetProperties(propFlags))
            {
                variableMap[p.Name] = p.PropertyType;
            }

            if (!string.IsNullOrEmpty(c))
            {
                bool skipTextAnalysis = type.IsInterface || type.IsEnum;

                if (!skipTextAnalysis)
                {
                    foreach (var scopedType in scope)
                    {
                        if (scopedType == type) continue;
                        var localMatches = Regex.Matches(c, $@"\b{scopedType.Name}\s+(\w+)");
                        foreach (Match m in localMatches)
                        {
                            string varName = m.Groups[1].Value;
                            if (!variableMap.ContainsKey(varName)) variableMap[varName] = scopedType;
                        }
                    }

                    var staticImports = new List<Type>();
                    if (rawUsings != null)
                    {
                        foreach (var u in rawUsings)
                        {
                            var foundType = scope.FirstOrDefault(t => u.EndsWith(t.Name));
                            if (foundType != null) staticImports.Add(foundType);
                        }
                    }

                    var singletonChainMatches = Regex.Matches(c, @"\b(\w+)\.(Instance|Shared|Main)\.(\w+)");
                    foreach (Match m in singletonChainMatches)
                    {
                        string className = m.Groups[1].Value;
                        string memberName = m.Groups[3].Value;
                        int line = classStartLine + GetLineFromIndex(c, m.Index) - 1;

                        var targetType = scope.FirstOrDefault(t => t.Name == className);
                        if (targetType != null && targetType != type)
                        {
                            list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.Instance, Description = memberName, SourceLine = line, SourcePath = currentFilePath });
                            if (IsDebugContext(c, m.Index))
                                list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.Debug, Description = memberName, SourceLine = line, SourcePath = currentFilePath });

                            AddMemberDependency(list, targetType, memberName, false, "", false, false, line, currentFilePath);
                        }
                    }

                    foreach (Match m in Regex.Matches(c, @"\bGetComponent\s*<\s*(\w+)\s*>\s*\("))
                    {
                        var t = scope.FirstOrDefault(x => x.Name == m.Groups[1].Value);
                        int line = classStartLine + GetLineFromIndex(c, m.Index) - 1;
                        if (t != null && t != type)
                        {
                            list.Add(new Dependency { TargetType = t, RelationType = RelationType.GetComponent, Description = "GetComponent", SourceLine = line, SourcePath = currentFilePath });
                            RegisterVariableFromAssignment(c, m.Index, t, variableMap);
                        }
                    }

                    foreach (Match m in Regex.Matches(c, @"\bFind(?:First|Any)?ObjectByType\s*<\s*(\w+)\s*>\s*\("))
                    {
                        var t = scope.FirstOrDefault(x => x.Name == m.Groups[1].Value);
                        int line = classStartLine + GetLineFromIndex(c, m.Index) - 1;
                        if (t != null && t != type)
                        {
                            list.Add(new Dependency { TargetType = t, RelationType = RelationType.GetComponent, Description = "FindObject", SourceLine = line, SourcePath = currentFilePath });

                            int endOfCall = c.IndexOf(')', m.Index);
                            if (endOfCall != -1 && endOfCall + 1 < c.Length && c[endOfCall + 1] == '.')
                            {
                                Match memberMatch = Regex.Match(c.Substring(endOfCall + 2), @"^(\w+)");
                                if (memberMatch.Success)
                                {
                                    string memberName = memberMatch.Groups[1].Value;
                                    bool isDebug = IsDebugContext(c, m.Index);
                                    AddMemberDependency(list, t, memberName, false, "", false, isDebug, line, currentFilePath);
                                }
                            }

                            RegisterVariableFromAssignment(c, m.Index, t, variableMap);
                        }
                    }

                    var dotMatches = Regex.Matches(c, @"\b(\w+(?:\[.*?\])?)\.(\w+)(?:\s*([\+\-\*\/]?=|\())?");

                    foreach (Match m in dotMatches)
                    {
                        string tokenObj = m.Groups[1].Value;
                        string tokenMember = m.Groups[2].Value;
                        string suffix = m.Groups[3].Success ? m.Groups[3].Value : "";
                        int line = classStartLine + GetLineFromIndex(c, m.Index) - 1;

                        if (tokenMember == "Instance" || tokenMember == "Shared" || tokenMember == "Main") continue;
                        if (tokenMember == "GetComponent") continue;

                        Type targetType = null;
                        bool isStaticAccess = false;

                        targetType = scope.FirstOrDefault(t => t.Name == tokenObj);
                        if (targetType != null) isStaticAccess = true;

                        if (targetType == null)
                        {
                            string cleanObjName = tokenObj.Contains("[") ? tokenObj.Substring(0, tokenObj.IndexOf('[')) : tokenObj;

                            if (variableMap.ContainsKey(cleanObjName))
                            {
                                targetType = variableMap[cleanObjName];

                                if (targetType.IsGenericType && targetType.GetGenericArguments().Length > 0)
                                {
                                    targetType = targetType.GetGenericArguments()[0];
                                }
                                else if (targetType.IsArray)
                                {
                                    targetType = targetType.GetElementType();
                                }
                            }
                        }

                        if (targetType != null && scope.Contains(targetType) && targetType != type)
                        {
                            if (targetType.IsEnum)
                            {
                                list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.Enum, Description = tokenMember, SourceLine = line, SourcePath = currentFilePath });
                                continue;
                            }

                            if (IsDebugContext(c, m.Index))
                                list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.Debug, Description = tokenMember, SourceLine = line, SourcePath = currentFilePath });

                            AddMemberDependency(list, targetType, tokenMember, isStaticAccess, suffix, false, false, line, currentFilePath);
                        }
                    }

                    if (staticImports.Count > 0)
                    {
                        var directMatches = Regex.Matches(c, @"\b(\w+)(?:\s*([\+\-\*\/]?=|\())?");
                        foreach (Match m in directMatches)
                        {
                            string memberName = m.Groups[1].Value;
                            string suffix = m.Groups[2].Success ? m.Groups[2].Value : "";
                            int line = classStartLine + GetLineFromIndex(c, m.Index) - 1;

                            if (variableMap.ContainsKey(memberName)) continue;
                            if (memberName == "if" || memberName == "for" || memberName == "foreach" || memberName == "while") continue;

                            foreach (var staticType in staticImports)
                            {
                                if (staticType == type) continue;
                                bool isDebug = IsDebugContext(c, m.Index);
                                AddMemberDependency(list, staticType, memberName, true, suffix, true, isDebug, line, currentFilePath);
                            }
                        }
                    }
                }
            }
            return list;
        }
        private void RegisterVariableFromAssignment(string code, int matchIndex, Type type, Dictionary<string, Type> variableMap)
        {
            int start = Math.Max(0, matchIndex - 50);
            string snippet = code.Substring(start, matchIndex - start);

            Match v = Regex.Match(snippet, @"\b(\w+)\s*=\s*$");
            if (v.Success)
            {
                string varName = v.Groups[1].Value;
                if (varName != "var" && varName != "return") variableMap[varName] = type;
            }
        }

        private void AddMemberDependency(List<Dependency> list, Type targetType, string memberName, bool isStaticAccess, string suffix, bool strictCheck, bool addDebug, int sourceLine, string sourcePath)
        {
            if (memberName == "Equals" || memberName == "ToString" || memberName == "GetHashCode" || memberName == "GetType") return;

            var memberInfos = targetType.GetMember(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (memberInfos.Length == 0)
            {
                if (strictCheck) return;

                if (suffix.Contains("("))
                    list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.MethodCall, Description = memberName, SourceLine = sourceLine, SourcePath = sourcePath });
                else
                {
                    if (suffix.Contains("=") && !suffix.Contains("=="))
                        list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.PropSet, Description = memberName, SourceLine = sourceLine, SourcePath = sourcePath });

                    if (!suffix.Contains("=") || suffix.Contains("+=") || suffix.Contains("-="))
                        list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.PropGet, Description = memberName, SourceLine = sourceLine, SourcePath = sourcePath });
                }
                return;
            }

            var info = memberInfos[0];

            if (info.MemberType == MemberTypes.Property || info.MemberType == MemberTypes.Field)
            {
                bool isCompound = suffix.Contains("+=") || suffix.Contains("-=") || suffix.Contains("++") || suffix.Contains("--");
                bool isSimpleSet = suffix.Contains("=") && !isCompound && !suffix.Contains("==") && !suffix.Contains("!=") && !suffix.Contains("=>");

                if (memberName == "Instance" || memberName == "Shared")
                    list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.Instance, Description = memberName, SourceLine = sourceLine, SourcePath = sourcePath });
                else if (isCompound)
                {
                    list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.PropGet, Description = memberName, SourceLine = sourceLine, SourcePath = sourcePath });
                    list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.PropSet, Description = memberName, SourceLine = sourceLine, SourcePath = sourcePath });
                }
                else if (isSimpleSet)
                    list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.PropSet, Description = memberName, SourceLine = sourceLine, SourcePath = sourcePath });
                else
                    list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.PropGet, Description = memberName, SourceLine = sourceLine, SourcePath = sourcePath });
            }
            else if (info.MemberType == MemberTypes.Method)
            {
                list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.MethodCall, Description = memberName, SourceLine = sourceLine, SourcePath = sourcePath });
            }
            else if (info.MemberType == MemberTypes.Event)
            {
                list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.EventSub, Description = memberName, SourceLine = sourceLine, SourcePath = sourcePath });
            }

            if (addDebug)
                list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.Debug, Description = memberName, SourceLine = sourceLine, SourcePath = sourcePath });

            if (isStaticAccess && info.MemberType != MemberTypes.NestedType)
            {
                bool isReallyStatic = false;
                if (info is FieldInfo fi) isReallyStatic = fi.IsStatic;
                else if (info is PropertyInfo pi) isReallyStatic = pi.GetGetMethod(true)?.IsStatic ?? false;
                else if (info is MethodInfo mi) isReallyStatic = mi.IsStatic;

                if (isReallyStatic)
                    list.Add(new Dependency { TargetType = targetType, RelationType = RelationType.StaticProp, Description = memberName, SourceLine = sourceLine, SourcePath = sourcePath });
            }
        }

        private bool IsDebugContext(string fullCode, int matchIndex)
        {
            int lineStart = fullCode.LastIndexOf('\n', matchIndex);
            if (lineStart == -1) lineStart = 0;
            string linePrefix = fullCode.Substring(lineStart, matchIndex - lineStart).TrimStart();

            if (linePrefix.StartsWith("Debug.") ||
                linePrefix.StartsWith("UnityEngine.Debug.") ||
                linePrefix.StartsWith("print("))
            {
                return true;
            }
            return false;
        }

        private string RemoveComments(string text)
        {
            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";

            return Regex.Replace(text,
                $"{blockComments}|{lineComments}|{strings}|{verbatimStrings}",
                me =>
                {
                    if (me.Value.StartsWith("//"))
                        return Environment.NewLine;
                    if (me.Value.StartsWith("/*"))
                    {
                        int lines = me.Value.Count(c => c == '\n');
                        return new string('\n', lines);
                    }
                    return me.Value;
                },
                RegexOptions.Singleline);
        }

        private TypeNode CreateBasicNode(Type type, string filePath, int lineNumber)
        {
            string prefix = "[Class] ";
            Color titleColor = new Color(0.2f, 0.2f, 0.2f);

            if (type.IsInterface)
            {
                prefix = "[Interface] ";
                titleColor = new Color(0.1f, 0.3f, 0.3f);
            }
            else if (type.IsEnum)
            {
                prefix = "[Enum] ";
                titleColor = new Color(0.45f, 0.35f, 0.5f);
            }
            else if (type.IsAbstract && type.IsSealed)
            {
                prefix = "[Static] ";
                titleColor = new Color(0.4f, 0.2f, 0.1f);
            }
            else if (type.IsAbstract)
            {
                prefix = "[Abstract] ";
            }
            else if (type.IsValueType && !type.IsEnum && !type.IsPrimitive)
            {
                prefix = "[Struct] ";
                titleColor = new Color(0.4f, 0.4f, 0.4f);
            }

            var node = new TypeNode(type, filePath, lineNumber)
            {
                title = prefix + type.Name + "  ",
                FullTypeName = type.FullName,
                tooltip = type.Namespace ?? "No namespace"
            };

            node.titleContainer.style.backgroundColor = titleColor;
            node.capabilities &= ~Capabilities.Collapsible;
            node.titleButtonContainer.style.display = DisplayStyle.None;

            node.RefreshExpandedState();
            node.RefreshPorts();
            return node;
        }
    }




    // ==================================================================
    // BoxNode
    // ==================================================================
    public class BoxNode : Node
    {
        public List<TypeNode> StoredNodes = new List<TypeNode>();
        private VisualElement _contentContainer;

        public BoxNode(string titleName)
        {
            title = titleName;
            style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            titleContainer.style.backgroundColor = new Color(0.6f, 0.3f, 0.1f);
            titleButtonContainer.style.display = DisplayStyle.None;
            capabilities &= ~Capabilities.Deletable;

            var label = titleContainer.Q<Label>("title-label");
            if (label != null)
            {
                label.style.flexGrow = 1;
            }

            var buttonGroup = new VisualElement();
            buttonGroup.style.flexDirection = FlexDirection.Row;

            var renameBtn = new Button(StartEditingTitle) { text = "Rename" };
            renameBtn.style.marginRight = 2; 

            var unpackBtn = new Button(UnpackAll) { text = "Unpack All" };

            buttonGroup.Add(renameBtn);
            buttonGroup.Add(unpackBtn);
            titleContainer.Add(buttonGroup);

            style.borderTopWidth = style.borderBottomWidth = style.borderLeftWidth = style.borderRightWidth = 2;
            style.borderTopColor = new Color(0.8f, 0.5f, 0.2f);

            _contentContainer = new VisualElement { style = { paddingLeft = 5, paddingRight = 5, marginTop = 5 } };
            extensionContainer.Add(_contentContainer);
            RefreshExpandedState();
        }

        private void StartEditingTitle()
        {
            var label = titleContainer.Q<Label>("title-label");
            if (label == null) return;

            label.style.display = DisplayStyle.None;

            var textField = new TextField { value = title };

            textField.style.flexGrow = 1;
            textField.style.marginLeft = 5;
            textField.style.marginRight = 5;

            var textInput = textField.Q("unity-text-input");
            textInput.style.backgroundColor = new StyleColor(Color.clear);
            textInput.style.borderTopWidth = 0;
            textInput.style.borderBottomWidth = 0;
            textInput.style.borderLeftWidth = 0;
            textInput.style.borderRightWidth = 0;
            textInput.style.color = Color.white;
            textInput.style.fontSize = 13;
            textInput.style.unityFontStyleAndWeight = FontStyle.Bold;

            titleContainer.Insert(titleContainer.IndexOf(label), textField);

            textField.Focus();
            textField.schedule.Execute(() => textField.SelectAll()).ExecuteLater(10);

            textField.RegisterCallback<FocusOutEvent>(evt => EndEditingTitle(label, textField));
            textField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return) textField.Blur();
                else if (evt.keyCode == KeyCode.Escape)
                {
                    textField.SetValueWithoutNotify(title);
                    textField.Blur();
                }
            });

            textField.RegisterCallback<MouseDownEvent>(e => e.StopPropagation());
        }

        private void EndEditingTitle(Label label, TextField textField)
        {
            if (textField.parent == null) return;

            title = textField.value;
            label.text = title;
            label.style.display = DisplayStyle.Flex;
            textField.RemoveFromHierarchy();
        }

        public Port GetOrCreatePort(Direction dir, string portName, Color color)
        {
            var container = dir == Direction.Output ? outputContainer : inputContainer;
            foreach (var child in container.Children())
                if (child is Port p && p.portName == portName) return p;

            var port = Port.Create<Edge>(Orientation.Horizontal, dir, Port.Capacity.Multi, typeof(bool));
            port.portName = portName;
            port.portColor = color;
            container.Add(port);
            RefreshPorts();
            return port;
        }

        public void RefreshContentList()
        {
            _contentContainer.Clear();
            if (StoredNodes.Count == 0)
            {
                _contentContainer.Add(new Label("Empty") { style = { color = Color.gray, fontSize = 10 } });
                return;
            }

            foreach (var node in StoredNodes.OrderBy(n => n.title))
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, marginBottom = 2 } };
                row.Add(new Label(node.title) { style = { fontSize = 10, width = 120, overflow = Overflow.Hidden } });
                row.Add(new Button(() => GetFirstAncestorOfType<DependencyGraphView>()?.ExtractNodeFromBox(this, node))
                {
                    text = "×",
                    style = { width = 18, height = 16, fontSize = 9, paddingLeft = 0, paddingRight = 0 }
                });
                _contentContainer.Add(row);
            }
        }

        private void UnpackAll() => GetFirstAncestorOfType<DependencyGraphView>()?.UnpackBox(this);
        public List<string> GetContainedTypeNames() => StoredNodes.Select(n => n.FullTypeName).ToList();
    }


    // ==================================================================
    // DependencyLineData
    // ==================================================================
    public struct DependencyLineData
    {
        public string DisplayText;  
        public string FilePath;  
        public int LineNumber;    
    }

    // ==================================================================
    // TypeNode 
    // ==================================================================
    public class TypeNode : Node
    {
        public string FullTypeName;
        public Rect CachedPosition;
        public int LineNumber; 

        public Port InPort_Top;
        public Port OutPort_Bottom;

        private readonly Type _type;
        private readonly string _filePath;

        private class DependencyData
        {
            public GraphElement TargetElement;
            public Color Color;
            public Port CurrentPort;
            public VisualElement CurrentContainer;
            public List<DependencyLineData> LineData;
        }

        private List<DependencyData> _dependencies = new List<DependencyData>();

        private VisualElement _topContainer;
        private VisualElement _bottomContainer;

        public TypeNode(Type type, string filePath, int lineNumber)
        {
            _type = type;
            _filePath = filePath;
            LineNumber = lineNumber;

            capabilities &= ~Capabilities.Deletable;

            _topContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.Center, paddingBottom = 2 } };
            mainContainer.Insert(0, _topContainer);

            _bottomContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.Center, paddingTop = 2 } };
            mainContainer.Add(_bottomContainer);

            InPort_Top = Port.Create<Edge>(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));

            InPort_Top.portName = "";
            InPort_Top.portColor = new Color(0.7f, 0.7f, 0.7f);
            InPort_Top.style.height = 24;
            _topContainer.Add(InPort_Top);

            OutPort_Bottom = Port.Create<Edge>(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            OutPort_Bottom.portName = "";
            OutPort_Bottom.portColor = new Color(0.7f, 0.7f, 0.7f);
            OutPort_Bottom.style.height = 24;
            OutPort_Bottom.style.flexDirection = FlexDirection.Column;
            _bottomContainer.Add(OutPort_Bottom);

            this.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.clickCount == 2) OpenInIDE(_filePath, LineNumber);
            });

            this.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (style.display != DisplayStyle.None)
                {
                    CachedPosition = GetPosition();
                    schedule.Execute(UpdateSmartRouting);
                    schedule.Execute(NotifyIncomingDependencies);
                }
            });
        }

        public Port MainInputPort => InPort_Top;

        public bool HasDependencyOn(TypeNode target)
        {
            foreach (var dep in _dependencies)
            {
                if (dep.TargetElement == target) return true;
            }
            return false;
        }

        private void NotifyIncomingDependencies()
        {
            var graphView = GetFirstAncestorOfType<GraphView>();
            if (graphView == null) return;

            foreach (var node in graphView.nodes)
            {
                if (node is TypeNode otherNode && otherNode != this)
                {
                    if (otherNode.HasDependencyOn(this))
                    {
                        otherNode.UpdateSmartRouting();
                    }
                }
            }
        }

        public Port AddDependencyPort(GraphElement target, List<DependencyLineData> lineData, Color blockColor)
        {
            string targetName = target is TypeNode tn ? tn.title : ((BoxNode)target).title;
            var (container, port) = CreatePortBlock(Direction.Output, targetName, lineData, blockColor);
            outputContainer.Add(container);

            _dependencies.Add(new DependencyData
            {
                TargetElement = target,
                Color = blockColor,
                CurrentPort = port,
                CurrentContainer = container,
                LineData = lineData
            });

            RefreshPorts();
            RefreshExpandedState();
            return port;
        }

        private (VisualElement container, Port port) CreatePortBlock(Direction direction, string targetName, List<DependencyLineData> lineData, Color blockColor)
        {
            var wrapper = new VisualElement();
            wrapper.style.backgroundColor = new Color(blockColor.r, blockColor.g, blockColor.b, 0.15f);
            wrapper.style.borderTopLeftRadius = 5;
            wrapper.style.borderTopRightRadius = 5;
            wrapper.style.borderBottomLeftRadius = 5;
            wrapper.style.borderBottomRightRadius = 5;
            wrapper.style.marginTop = 4;
            wrapper.style.marginBottom = 4;
            wrapper.style.paddingTop = 2;
            wrapper.style.paddingBottom = 2;

            if (direction == Direction.Output)
            {
                wrapper.style.paddingRight = 4;
                wrapper.style.paddingLeft = 10; 
            }
            else
            {
                wrapper.style.paddingLeft = 4;
                wrapper.style.paddingRight = 10;
            }

            var header = new Label(direction == Direction.Output ? $"{targetName}    →" : $"←     {targetName}");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.color = new Color(0.9f, 0.9f, 0.9f);
            header.style.marginBottom = 3;
            header.style.alignSelf = direction == Direction.Output ? Align.FlexEnd : Align.FlexStart;

            wrapper.Add(header);

            Port.Capacity capacity = Port.Capacity.Multi;
            var port = Port.Create<Edge>(Orientation.Horizontal, direction, capacity, typeof(bool));
            port.portName = "";
            port.portColor = blockColor;
            port.style.height = StyleKeyword.Auto;
            port.style.paddingTop = 4;
            port.style.paddingBottom = 4;

            var defaultLabel = port.Q<Label>();
            if (defaultLabel != null) defaultLabel.style.display = DisplayStyle.None;

            var linesContainer = new VisualElement();
            linesContainer.style.flexDirection = FlexDirection.Column;
            linesContainer.style.justifyContent = Justify.Center;

            foreach (var data in lineData)
            {
                var l = new Label(data.DisplayText);
                l.style.fontSize = 11;
                l.style.marginBottom = 2;
                l.enableRichText = true;

                l.style.alignSelf = direction == Direction.Input ? Align.FlexStart : Align.FlexEnd;

                l.style.unityFontStyleAndWeight = FontStyle.Bold;

                l.schedule.Execute(() =>
                {
                    if (l.resolvedStyle.width > 0)
                    {
                        l.style.minWidth = l.resolvedStyle.width;

                        l.style.unityFontStyleAndWeight = FontStyle.Normal;
                    }
                    else
                    {
                        l.style.unityFontStyleAndWeight = FontStyle.Normal;
                    }
                });

                l.RegisterCallback<MouseEnterEvent>(evt => l.style.unityFontStyleAndWeight = FontStyle.Bold);
                l.RegisterCallback<MouseLeaveEvent>(evt => l.style.unityFontStyleAndWeight = FontStyle.Normal);

                l.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.button == 0)
                    {
                        OpenInIDE(data.FilePath, data.LineNumber);
                        evt.StopPropagation();
                    }
                });

                linesContainer.Add(l);
            }

            if (direction == Direction.Input)
            {
                port.style.flexDirection = FlexDirection.Row;
                port.Add(linesContainer);
                linesContainer.style.paddingLeft = 5;
            }
            else
            {
                port.style.flexDirection = FlexDirection.RowReverse;
                port.Add(linesContainer);
                linesContainer.style.paddingRight = 5;
            }

            wrapper.Add(port);

            return (wrapper, port);
        }

        public void UpdateSmartRouting()
        {
            if (parent == null || style.display == DisplayStyle.None || _dependencies.Count == 0) return;

            foreach (var dep in _dependencies)
            {
                if (dep.TargetElement == null) continue;

                bool isBox = dep.TargetElement is BoxNode;
                bool isHiddenNode = dep.TargetElement is TypeNode nodeCheck && nodeCheck.parent == null;

                if (isBox || isHiddenNode)
                {
                    continue;
                }

                Vector2 myPos = GetPosition().position;
                Vector2 targetPos = dep.TargetElement.GetPosition().position;
                bool isTargetLeft = (targetPos.x + 50) < myPos.x;

                string targetName = dep.TargetElement is TypeNode tNode ? tNode.title : ((BoxNode)dep.TargetElement).title;

                if (isTargetLeft && dep.CurrentPort.direction != Direction.Input)
                    ReplacePortBlock(dep, Direction.Input, inputContainer, targetName);
                else if (!isTargetLeft && dep.CurrentPort.direction != Direction.Output)
                    ReplacePortBlock(dep, Direction.Output, outputContainer, targetName);

                if (dep.TargetElement is TypeNode targetNode)
                {
                    ConnectEdgeSafe(isTargetLeft ? targetNode.OutPort_Bottom : dep.CurrentPort,
                                    isTargetLeft ? dep.CurrentPort : targetNode.InPort_Top, dep.Color);
                }
            }

            SortPorts();
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            CachedPosition = newPos;
            schedule.Execute(UpdateSmartRouting);
            schedule.Execute(NotifyIncomingDependencies);
            schedule.Execute(SortPorts);
        }

        private void SortPorts()
        {
            SortContainer(inputContainer);
            SortContainer(outputContainer);

            void SortContainer(VisualElement container)
            {
                var wrappers = new List<VisualElement>();
                foreach (var child in container.Children())
                {
                    if (_dependencies.Any(d => d.CurrentContainer == child))
                        wrappers.Add(child);
                }

                if (wrappers.Count == 0) return;

                Vector2 myCenter = GetPosition().center;

                var wrappersToSort = new List<VisualElement>(wrappers);
                wrappersToSort.Sort((wA, wB) =>
                {
                    float scoreA = GetSortScore(wA, myCenter);
                    float scoreB = GetSortScore(wB, myCenter);
                    return scoreA.CompareTo(scoreB);
                });

                foreach (var w in wrappersToSort)
                {
                    w.BringToFront();
                }
            }
        }

        private float GetSortScore(VisualElement wrapper, Vector2 myCenter)
        {
            var dep = _dependencies.Find(d => d.CurrentContainer == wrapper);

            if (dep == null || dep.TargetElement == null) return 0;

            Vector2 targetCenter = dep.TargetElement.GetPosition().center;
            float distance = Vector2.Distance(myCenter, targetCenter);

            if (targetCenter.y < myCenter.y) return -1000000f + distance;
            else return 1000000f - distance;
        }

        private void ReplacePortBlock(DependencyData dep, Direction newDirection, VisualElement newContainer, string targetName)
        {
            if (dep.CurrentContainer != null)
            {
                if (dep.CurrentPort != null && dep.CurrentPort.connected)
                {
                    var edgesToDelete = dep.CurrentPort.connections.ToList();
                    foreach (var edge in edgesToDelete)
                    {
                        edge.input?.Disconnect(edge);
                        edge.output?.Disconnect(edge);
                        edge.RemoveFromHierarchy();
                    }
                }
                dep.CurrentContainer.RemoveFromHierarchy();
            }

            var (newWrapper, newPort) = CreatePortBlock(newDirection, targetName, dep.LineData, dep.Color);

            dep.CurrentPort = newPort;
            dep.CurrentContainer = newWrapper;

            newContainer.Add(newWrapper);

            RefreshPorts();
        }

        private void ConnectEdgeSafe(Port output, Port input, Color color)
        {
            if (output == null || input == null) return;

            foreach (var edge in output.connections)
            {
                if (edge.input == input) return;
            }

            schedule.Execute(() =>
            {
                schedule.Execute(() =>
                {
                    if (output.parent == null || input.parent == null) return;
                    foreach (var edge in output.connections) if (edge.input == input) return;

                    var newEdge = new Edge { output = output, input = input };
                    newEdge.input.Connect(newEdge);
                    newEdge.output.Connect(newEdge);
                    newEdge.output.portColor = color;
                    newEdge.input.portColor = color;

                    var graphView = GetFirstAncestorOfType<GraphView>();
                    if (graphView != null)
                    {
                        graphView.AddElement(newEdge);
                    }
                });
            });
        }

        private void OpenInIDE(string path, int line)
        {
            if (!string.IsNullOrEmpty(path))
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (asset != null)
                {
                    AssetDatabase.OpenAsset(asset, line);
                }
            }
        }
    }

    // ==================================================================
    // GRAPH VIEW
    // ==================================================================
    public class DependencyGraphView : GraphView
    {
        public bool FocusModeEnabled = true;
        private ClassDependencyGraph _editorWindow;

        private Vector2 _mouseDownScreenPos;
        private bool _isClickingItem;

        private const float  MIN_SCALE = 0.1f;
        private const float Max_SCALE = 1.5f;

        public DependencyGraphView(ClassDependencyGraph window)
        {
            _editorWindow = window;
            SetupZoom(MIN_SCALE, Max_SCALE);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            grid.style.backgroundColor = new Color(0.17f, 0.17f, 0.17f);
            Insert(0, grid);
            grid.StretchToParentSize();

            var miniMap = new MiniMap { anchored = true };
            miniMap.SetPosition(new Rect(10, 50, 200, 140));
            miniMap.style.backgroundColor = new Color(0, 0, 0, 0.5f);
            Add(miniMap);

            RegisterCallback<MouseDownEvent>(OnMouseDownEvent, TrickleDown.TrickleDown);
            RegisterCallback<MouseUpEvent>(OnMouseUpEvent, TrickleDown.TrickleDown);
        }

        private void OnMouseDownEvent(MouseDownEvent evt)
        {
            _mouseDownScreenPos = evt.mousePosition;

            var targetElement = evt.target as VisualElement;

            bool hitNode = targetElement != null && (targetElement is Node || targetElement.GetFirstAncestorOfType<Node>() != null);
            bool hitGroup = targetElement != null && (targetElement is Group || targetElement.GetFirstAncestorOfType<Group>() != null);

            _isClickingItem = hitNode || hitGroup;
        }

        private void OnMouseUpEvent(MouseUpEvent evt)
        {
            float distance = Vector2.Distance(_mouseDownScreenPos, evt.mousePosition);

            if (_isClickingItem && distance > 10f)
            {
                schedule.Execute(ClearSelection).ExecuteLater(50);
            }

            _isClickingItem = false;
        }

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);
            RefreshFocusMode();
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            RefreshFocusMode();
        }

        public override void ClearSelection()
        {
            base.ClearSelection();
            RefreshFocusMode();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var typeNodes = selection.OfType<TypeNode>().ToList();
            var boxNodes = selection.OfType<BoxNode>().ToList();

            evt.menu.AppendAction("Create Sticky Group", (a) => CreateGroupFromSelection());

            var nodesInGroups = typeNodes.Where(n => GetContainingGroup(n) != null).ToList();
            if (nodesInGroups.Count > 0)
            {
                evt.menu.AppendAction("Remove from Group", (a) => RemoveNodesFromGroups(nodesInGroups));
            }

            if (typeNodes.Count > 0 && boxNodes.Count == 0)
                evt.menu.AppendAction("Pack into Box", (a) => PackSelectionIntoBox());

            if (typeNodes.Count > 0 && boxNodes.Count == 1)
            {
                var targetBox = boxNodes[0];
                evt.menu.AppendAction($"Add to '{targetBox.title}'", (a) => AddSelectionToBox(targetBox, typeNodes));
            }
        }

        public void PackSelectionIntoBox()
        {
            var nodesToPack = selection.OfType<TypeNode>().ToList();
            if (nodesToPack.Count == 0) return;

            Vector2 center = Vector2.zero;
            foreach (var n in nodesToPack) center += n.GetPosition().position;
            center /= nodesToPack.Count;

            PackNodesIntoBox(nodesToPack, "New Box", center);
        }

        public void CreateGroupFromSelection()
        {
            var nodesToGroup = selection.OfType<TypeNode>().ToList();
            Vector2 pos = Vector2.zero;

            if (nodesToGroup.Count == 0)
            {
                pos = viewTransform.position * -1 + new Vector3(200, 200, 0);
            }

            CreateGroupFromSelection("New Group", pos, nodesToGroup);
        }

        public void CreateGroupFromSelection(string title, Vector2 pos, List<TypeNode> nodes)
        {
            var group = new Group { title = title };

            group.headerContainer.style.backgroundColor = new Color(0.8f, 0.4f, 0.1f, 0.8f);

            var titleLabel = group.headerContainer.Q<Label>();
            if (titleLabel != null)
            {
                titleLabel.style.whiteSpace = WhiteSpace.Normal;
                titleLabel.style.fontSize = 14;
                titleLabel.style.color = Color.white;
            }

            if (nodes != null && nodes.Count > 0)
            {
                foreach (var node in nodes) group.AddElement(node);
            }
            else
            {
                group.SetPosition(new Rect(pos, Vector2.zero));
            }

            AddElement(group);
        }

        public void UnpackBox(BoxNode box)
        {
            ClearSelection();
            RemoveElement(box);
            TriggerGraphRefresh();
        }

        public void ExtractNodeFromBox(BoxNode box, TypeNode nodeToExtract)
        {
            if (box.StoredNodes.Contains(nodeToExtract))
            {
                box.StoredNodes.Remove(nodeToExtract);

                var boxPos = box.GetPosition();
                nodeToExtract.CachedPosition = new Rect(boxPos.x + 20, boxPos.y + 20, 0, 0);

                TriggerGraphRefresh();
            }
        }

        public void AddSelectionToBox(BoxNode box, List<TypeNode> nodesToAdd)
        {
            box.StoredNodes.AddRange(nodesToAdd);

            ClearSelection();

            foreach (var node in nodesToAdd)
            {
                node.CachedPosition = node.GetPosition();
            }

            TriggerGraphRefresh();
        }

        public void PackNodesIntoBox(List<TypeNode> nodesToPack, string title, Vector2 position)
        {
            var box = new BoxNode(title);
            box.StoredNodes = nodesToPack;
            box.SetPosition(new Rect(position, Vector2.zero));
            AddElement(box);

            ClearSelection();
            foreach (var node in nodesToPack)
            {
                node.CachedPosition = node.GetPosition();
                if (Contains(node)) RemoveElement(node);
            }

            TriggerGraphRefresh();
        }


        private void TriggerGraphRefresh()
        {
            EditorApplication.delayCall += () =>
            {
                if (_editorWindow != null)
                {
                    _editorWindow.TriggerRefreshFromView();
                }
            };
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

        public void ApplySearch(string search)
        {
            ClearSelection();

            if (string.IsNullOrEmpty(search))
            {
                foreach (var el in graphElements) el.style.opacity = 1f;
                RefreshFocusMode();
                return;
            }

            search = search.ToLowerInvariant();

            foreach (var node in nodes.OfType<TypeNode>())
            {
                bool matches = node.title.ToLowerInvariant().Contains(search);
                node.style.opacity = matches ? 1f : 0.15f;
                if (matches) AddToSelection(node);
            }

            foreach (var box in nodes.OfType<BoxNode>())
            {
                bool matchTitle = box.title.ToLowerInvariant().Contains(search);
                bool matchContent = box.StoredNodes.Any(n => n.title.ToLowerInvariant().Contains(search));
                bool matches = matchTitle || matchContent;
                box.style.opacity = matches ? 1f : 0.15f;
            }

            if (selection.Count > 0)
            {
                FrameSelection();
                RefreshFocusMode();
            }
        }

        public void RefreshFocusMode()
        {
            if (!FocusModeEnabled)
            {
                foreach (var el in graphElements) el.style.opacity = 1f;
                return;
            }

            if (selection == null || selection.Count == 0)
            {
                foreach (var el in graphElements) el.style.opacity = 1f;
                return;
            }

            foreach (var el in graphElements) el.style.opacity = 0.1f;

            var nodesInFocus = new HashSet<Node>();
            var nodesToLightUp = new HashSet<Node>();

            var brightEdges = new HashSet<Edge>();
            var brightGroups = new HashSet<Group>();

            foreach (var sel in selection)
            {
                if (sel is Node node)
                {
                    nodesInFocus.Add(node);
                    nodesToLightUp.Add(node);
                    var group = GetContainingGroup(node);
                    if (group != null) brightGroups.Add(group);
                }
                else if (sel is Group group)
                {
                    brightGroups.Add(group);
                    foreach (var element in group.containedElements)
                    {
                        if (element is Node gn)
                        {
                            nodesInFocus.Add(gn);
                            nodesToLightUp.Add(gn);
                        }
                    }
                }
                else if (sel is Edge edge)
                {
                    brightEdges.Add(edge);

                    if (edge.input?.node is Node inNode)
                    {
                        nodesToLightUp.Add(inNode);
                        var g = GetContainingGroup(inNode);
                        if (g != null) brightGroups.Add(g);
                    }

                    if (edge.output?.node is Node outNode)
                    {
                        nodesToLightUp.Add(outNode);
                        var g = GetContainingGroup(outNode);
                        if (g != null) brightGroups.Add(g);
                    }
                }
            }

            if (nodesInFocus.Count > 0)
            {
                foreach (var edge in edges)
                {
                    var inputNode = edge.input?.node as Node;
                    var outputNode = edge.output?.node as Node;

                    if (inputNode == null || outputNode == null) continue;

                    bool inIsFocus = nodesInFocus.Contains(inputNode);
                    bool outIsFocus = nodesInFocus.Contains(outputNode);

                    if (inIsFocus ^ outIsFocus)
                    {
                        brightEdges.Add(edge);
                        nodesToLightUp.Add(inputNode);
                        nodesToLightUp.Add(outputNode);
                    }
                }
            }

            foreach (var node in nodesToLightUp) node.style.opacity = 1f;
            foreach (var edge in brightEdges) edge.style.opacity = 1f;

            foreach (var group in graphElements.OfType<Group>())
            {
                group.style.opacity = brightGroups.Contains(group) ? 1f : 0.4f;
            }
        }

        public void RemoveNodesFromGroups(List<TypeNode> nodes)
        {
            foreach (var node in nodes)
            {
                var group = GetContainingGroup(node);
                if (group != null)
                {
                    group.RemoveElement(node);
                }
            }
        }

        private Group GetContainingGroup(Node node)
        {
            foreach (var g in graphElements.OfType<Group>())
            {
                if (g.ContainsElement(node)) return g;
            }
            return null;
        }
    }
}