﻿namespace ProteoformSuiteGUI
{
    public abstract class DisplayObject
    {
        #region Public Field

        public object display_object;

        #endregion Public Field

        #region Public Constructor

        public DisplayObject(object o)
        {
            display_object = o;
        }

        #endregion Public Constructor
    }
}