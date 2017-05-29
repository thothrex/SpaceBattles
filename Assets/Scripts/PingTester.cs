using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class PingTester : MonoBehaviour
    {
        public Text OutputTextBox;
        private static readonly int FPSUpdateInterval = 1000; // in ms
        // can't be valid if shorter than this
        private static readonly int MinimumIPAddressLength = 2;
        private static readonly int ReadWriteLockTimeout = 100; // ms
        private static readonly string FPSDisplaySuffix = "ms";
        private Stopwatch timer = new Stopwatch();
        private bool _ShouldTest = false;
        private bool PingInProgressValue = false;
        private ReaderWriterLockSlim PingInProgressLock = new ReaderWriterLockSlim();
        
        public string TestingIPAddress { get; set; }
        public bool ShouldTest {
            get { return _ShouldTest; }
            set
            {
                if (value
                && (    TestingIPAddress == null
                    || (TestingIPAddress.Length < MinimumIPAddressLength))
                )
                {
                    throw new InvalidOperationException(
                        "Need to set an ip before enabling testing"
                    );
                }
                else
                {
                    _ShouldTest = value;
                    if (value)
                    {
                        timer.Start();
                    }
                    else
                    {
                        timer.Stop();
                        OutputTextBox.text = "[Inactive]";
                    }
                }
            }
        }

        public void Start ()
        {
            MyContract.RequireFieldNotNull(OutputTextBox, "Output Textbox");
            timer.Start();
        }

        public void Update ()
        {
            if (ShouldTest)
            {
                //if (!Network.isClient && !Network.isServer)
                //{
                //    // Disconnected
                //    OutputTextBox.text = "[Inactive]";
                //    return;
                //}

                if (timer.ElapsedMilliseconds >= FPSUpdateInterval)
                {
                    UnityEngine.Debug.Log("timer.ElapsedMilliseconds >= FPSUpdateInterval");
                    timer.Reset();
                    timer.Start();
                    StartCoroutine(DisplayPingIn(OutputTextBox));
                }
            }
        }

        private IEnumerator DisplayPingIn (Text outputDestination)
        {
            UnityEngine.Debug.Log("DisplayPingIn: Start");
            if (ShouldTest)
            {
                UnityEngine.Debug.Log("DisplayPingIn: ShouldTest");
                // concurrent check
                if (!PingInProgress())
                {
                    UnityEngine.Debug.Log("DisplayPingIn: !PingInProgress()");
                    if (!Network.isServer && !TestingIPAddress.Equals("localhost"))
                    {
                        UnityEngine.Debug.Log("DisplayPingIn: !Network.isServer");
                        SetPingInProgress(true, ReadWriteLockTimeout);
                        UnityEngine.Debug.Log("DisplayPingIn: Creating ping to address " + TestingIPAddress);
                        Ping PingObject = new Ping(TestingIPAddress);
                        yield return new WaitUntil(() => PingObject.isDone);

                        OutputTextBox.text = PingObject.time.ToString()
                                           + FPSDisplaySuffix;
                        SetPingInProgress(false, ReadWriteLockTimeout);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("DisplayPingIn: Network.isServer");
                        OutputTextBox.text = "(Server) 0"
                                           + FPSDisplaySuffix;
                        //ShouldTest = false;
                    }
                }
            }
        }

        private bool PingInProgress ()
        {
            PingInProgressLock.EnterReadLock();
            try
            {
                return PingInProgressValue;
            }
            finally
            {
                PingInProgressLock.ExitReadLock();
            }
        }

        private bool SetPingInProgress(bool newStatus, int timeout)
        {
            if (PingInProgressLock.TryEnterWriteLock(timeout))
            {
                try
                {
                    PingInProgressValue = newStatus;
                }
                finally
                {
                    PingInProgressLock.ExitWriteLock();
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

