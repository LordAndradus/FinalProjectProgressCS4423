using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

//Creating a neat little timer class now, so I can reuse it in my final project :^)

public class Timer
{
    public event Action TimerFunction; //To be called when the timer finishes. Must be delegated in calling class.

    public float TotalElapsedTime = 0f;
    public float duration;
    public float elapsedTime = 0f;

    bool isRunning;

    public Timer(float duration, bool running = true, Action Callback = null)
    {
        setDuration(duration);
        isRunning = running;
        if(Callback != null) TimerFunction += Callback;
    }

    public void start()
    {
        isRunning = true;
    }

    public void stop()
    {
        isRunning = false;
    }

    public void stopReset()
    {
        elapsedTime = 0f;
        stop();
    }

    public void stopReset(float duration)
    {
        stopReset();
        setDuration(duration);
    }
    
    public void reset()
    {
        elapsedTime = 0f;
        start();
    }

    public void reset(float duration)
    {
        reset();
        setDuration(duration);
    }

    public void update()
    {
        if(isRunning) 
        {
            //Using delta time to keep track of timings
            elapsedTime += Time.deltaTime; 
            TotalElapsedTime += Time.deltaTime;

            if(elapsedTime >= duration)
            {
                elapsedTime -= duration;
                isRunning = false;
                TimerFunction?.Invoke(); //Send out signal to launch any delegates
            }
        }
    }

    public void setDuration(float duration)
    {
        if(duration < 0.0f) 
        {
            duration *= -1; //what
            Debug.Log("NEAGTIVE detected in Timer class; " + ToString() + "\nSet to positive value of " + duration);
        }

        this.duration = duration;
    }

    public bool isTimerRunning()
    {
        return isRunning;
    }

    public override String ToString()
    {
        return "Timer, Hash Code = " + this.GetHashCode() + " | Duration = " + duration + " | Delegate address = " + Marshal.GetFunctionPointerForDelegate(TimerFunction);
    }
}
