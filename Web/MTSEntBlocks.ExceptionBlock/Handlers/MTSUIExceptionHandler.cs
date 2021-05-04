using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System;



namespace MTSEntBlocks.ExceptionBlock.Handlers
{
    public static class MTSUIExceptionHandler
    {
        #region Static Variables

        private static readonly ExceptionPolicyFactory _exceptionPolicyFactory;
        private static readonly ExceptionManager _exceptionManager;

        #endregion

        #region Constructor

        static MTSUIExceptionHandler()
        {
            SetLogWriter.Create();
            _exceptionPolicyFactory = new ExceptionPolicyFactory();
            _exceptionManager = _exceptionPolicyFactory.CreateManager();
        }

        #endregion

        public static void HandleException(ref Exception ex)
        {
            Exception exceptionToThrow = null;

            if (_exceptionManager.HandleException(ex, "MTSUIExceptionHandling", out exceptionToThrow))
            {
                if (exceptionToThrow == null)
                    throw ex;
                else
                    throw exceptionToThrow;
            }

        }

        //public static void LogException(ref Exception ex)
        //{
        //    _exceptionManager.HandleException(ex, "UIExceptionLogAndRethrowPolicy");
        //}

        //public static void Write(string msg, string handler)
        //{
        //    Logger.Write(msg, handler);
        //}

        ////public static bool HandleException(ref System.Exception ex)
        ////{
        ////    return true;
        ////    //bool rethrow = false;
        ////    //try
        ////    //{
        ////    //    if (ex is MTSException)
        ////    //    {
        ////    //        // DA BL exception has already been logged / handled
        ////    //    }
        ////    //    else
        ////    //    {
        ////    //        rethrow = ExceptionPolicy.HandleException(ex, "UserInterfacePolicy");
        ////    //    }
        ////    //}
        ////    //catch (Exception exp)
        ////    //{
        ////    //    ex = exp;
        ////    //}
        ////    //return rethrow;
        ////}
    }
}
