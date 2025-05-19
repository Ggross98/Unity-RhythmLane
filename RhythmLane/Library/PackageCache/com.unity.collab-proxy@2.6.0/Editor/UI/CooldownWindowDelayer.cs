using System;
using System.ComponentModel;
using UnityEditor;

namespace Unity.PlasticSCM.Editor.UI
{
    // Internal usage. This isn't a public API.
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CooldownWindowDelayer
    {
        internal static bool IsUnitTesting { get; set; }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CooldownWindowDelayer(Action action, double cooldownSeconds)
        {
            mAction = action;
            mCooldownSeconds = cooldownSeconds;
        }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Ping()
        {
            if (IsUnitTesting)
            {
                mAction();
                return;
            }

            if (mIsOnCooldown)
            {
                RefreshCooldown();
                return;
            }

            StartCooldown();
        }

        void RefreshCooldown()
        {
            mIsOnCooldown = true;

            mSecondsOnCooldown = mCooldownSeconds;
        }

        void StartCooldown()
        {
            mLastUpdateTime = EditorApplication.timeSinceStartup;

            EditorApplication.update += OnUpdate;

            RefreshCooldown();
        }

        void EndCooldown()
        {
            EditorApplication.update -= OnUpdate;

            mIsOnCooldown = false;

            mAction();
        }

        void OnUpdate()
        {
            double updateTime = EditorApplication.timeSinceStartup;
            double deltaSeconds = updateTime - mLastUpdateTime;

            mSecondsOnCooldown -= deltaSeconds;

            if (mSecondsOnCooldown < 0)
                EndCooldown();

            mLastUpdateTime = updateTime;
        }

        readonly Action mAction;
        readonly double mCooldownSeconds;

        double mLastUpdateTime;
        bool mIsOnCooldown;
        double mSecondsOnCooldown;
    }
}
