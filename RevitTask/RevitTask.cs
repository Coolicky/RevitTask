﻿using Autodesk.Revit.UI;
using System;
using System.Threading.Tasks;

/// <summary>
/// <see cref="System.Threading.Tasks.Task"/> wrapper
/// for <see cref="Autodesk.Revit.UI.IExternalEventHandler"/>
/// </summary>
/// <typeparam name="TResult"></typeparam>
public class RevitTask<TResult>
{
    private EventHandler _handler;
    private TaskCompletionSource<TResult> _tcs;
    private ExternalEvent _externalEvent;

    /// <summary>
    /// Sets required <paramref name="func"/> as a body
    /// of <see cref="IExternalEventHandler.Execute(UIApplication)"/>
    /// method and raises related <see cref="Autodesk.Revit.UI.ExternalEvent"/>
    /// </summary>
    /// <param name="func">Any function that depends on
    /// <see cref="Autodesk.Revit.UI.UIApplication"/>
    /// and results in object of <see cref="TResult"/> type.</param>
    /// <param name="continueOnCapturedContext">This parameter
    /// can be used to control the thread, on which
    /// continuation will take place.</param>
    public RevitTask()
    {
        _handler = new EventHandler();

        _handler.EventCompleted += OnEventCompleted;

        _externalEvent = ExternalEvent.Create(_handler);
    }

    /// <summary>
    /// Sets required <paramref name="func"/> as a body
    /// of <see cref="IExternalEventHandler.Execute(UIApplication)"/>
    /// method and raises related <see cref="Autodesk.Revit.UI.ExternalEvent"/>
    /// </summary>
    /// <param name="func">Any function that depends on
    /// <see cref="Autodesk.Revit.UI.UIApplication"/>
    /// and results in object of <see cref="TResult"/> type.</param>
    /// <param name="continueOnCapturedContext">This parameter
    /// can be used to control the thread, on which
    /// continuation will take place.</param>
    public Task<TResult> Run(Func<UIApplication, TResult> func)
    {
        _tcs = new TaskCompletionSource<TResult>();

        _handler.Func = func;

        _externalEvent.Raise();

        return _tcs.Task;
    }

    /// <summary>
    /// Sets Task result to object of TResult type or Exception
    /// after <see cref="IExternalEventHandler.Execute(UIApplication)"/>
    /// completes.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="result"></param>
    private void OnEventCompleted(object sender, TResult result)
    {
        if (_handler.Exception == null)
        {
            _tcs.TrySetResult(result);
        }
        else
        {
            _tcs.TrySetException(_handler.Exception);
        }
    }

    private class EventHandler :
        IExternalEventHandler
    {
        private Func<UIApplication, TResult> _func;

        public event EventHandler<TResult> EventCompleted;

        public Exception Exception { get; private set; }

        public Func<UIApplication, TResult> Func
        {
            get => _func;
            set => _func = value ??
                throw new ArgumentNullException();
        }

        public void Execute(UIApplication app)
        {
            TResult result = default;
            try
            {
                result = Func(app);
            }
            catch (Exception ex)
            {
                Exception = ex;
            }

            EventCompleted?.Invoke(this, result);
        }

        public string GetName()
        {
            return "some func";
        }
    }
}