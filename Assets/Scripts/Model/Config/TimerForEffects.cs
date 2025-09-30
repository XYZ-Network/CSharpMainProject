using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class TimerForEffects
{
    private static System.Timers.Timer _timer;

    //public static void Main(string[] args)
    //{
    //    SetTimer();

    //    Console.WriteLine("Нажмите любую клавишу для выхода.");
    //    Console.ReadKey();
    //    _timer.Stop();
    //    _timer.Dispose();
    //}

    //private static void SetTimer(int durationInMs)
    //{
    //    // Создаем таймер, который будет срабатывать каждую секунду.
    //    _timer = new System.Timers.Timer(durationInMs);
    //    // Hook up the Elapsed event for the timer.
    //    _timer.Elapsed += OnTimedEvent;
    //    _timer.AutoReset = false;
    //    _timer.Enabled = true;
    //}

    //private static void OnTimedEvent(System.Object source, ElapsedEventArgs e)
    //{
    //    Console.WriteLine("Событие, вызванное таймером в {0:HH:mm:ss.fff}", e.SignalTime);
    //}
}
