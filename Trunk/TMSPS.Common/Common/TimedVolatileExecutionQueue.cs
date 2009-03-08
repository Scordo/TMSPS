using System;
using System.Threading;
using System.Collections.Generic;

namespace TMSPS.Core.Common
{
    public delegate void ParameterizedMethod<T>(T parameter);

    public class TimedVolatileExecutionQueue<TMethodParameter>
    {
        #region Non Public Members

        private readonly object _stackModifyLockObject = new object();

        #endregion

        #region Properties

        private TimeSpan Interval { get; set; }
        private Stack<TimedVolatileExecutionQueueItem<TMethodParameter>> Stack { get; set; }
        private Timer InternalTimer { get; set; }
        

        #endregion

        #region Constructor

        public TimedVolatileExecutionQueue(TimeSpan interval) 
        {
            if (interval.TotalMilliseconds <= 0)
                throw new ArgumentOutOfRangeException("interval", "Please provide an interval with more than 0 milliseconds.");

            Interval = interval;
            Stack = new Stack<TimedVolatileExecutionQueueItem<TMethodParameter>>();
            InternalTimer = new Timer(OnTimerElapsed, null, Interval, Interval);
        }

        #endregion

        #region Public Methods

        public void Enqueue(ParameterizedMethod<TMethodParameter> methodToExecute, TMethodParameter methodParameter)
        {
            lock (_stackModifyLockObject)
            {
                Stack.Push(new TimedVolatileExecutionQueueItem<TMethodParameter>(methodToExecute, methodParameter));
            }
        }

        public void Clear()
        {
            lock (_stackModifyLockObject)
            {
                Stack.Clear();
            }
        }

        public void Stop()
        {
            if (InternalTimer != null)
                InternalTimer.Dispose();
        }

        #endregion

        #region Non Public Methods

        private void OnTimerElapsed(object state)
        {
            if (Stack.Count == 0)
                return;

            TimedVolatileExecutionQueueItem<TMethodParameter> item;

            lock (_stackModifyLockObject)
            {
                item = Stack.Pop();
                Stack.Clear();    
            }

            item.MethodToExecute(item.MethodParameter);
        }

        #endregion

        #region Embedded Classes

        private class TimedVolatileExecutionQueueItem<TParameter>
        {
            #region Properties

            public ParameterizedMethod<TParameter> MethodToExecute { get; private set; }
            public TParameter MethodParameter { get; private set; }

            #endregion

            #region Constructor

            public TimedVolatileExecutionQueueItem(ParameterizedMethod<TParameter> methodToExecute, TParameter methodParameter)
            {
                if (methodToExecute == null)
                    throw new ArgumentNullException("methodToExecute");


                MethodToExecute = methodToExecute;
                MethodParameter = methodParameter;
            }

            #endregion
        }

        #endregion
    }
}