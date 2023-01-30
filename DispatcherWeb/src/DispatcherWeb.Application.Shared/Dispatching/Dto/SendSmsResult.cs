using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatcherWeb.Dispatching.Dto
{
    public class SendSmsResult
    {
        
    }

	public class SendSmsResultSuccess : SendSmsResult
	{
	}

    public class SendSmsResultNextDispatch : SendSmsResult
    {
        public SendSmsResultNextDispatch(Guid nextDispatchGuid)
        {
            NextDispatchGuid = nextDispatchGuid;
        }
        public Guid NextDispatchGuid { get; set; }
    }

    public class SendSmsResultNoDispatch : SendSmsResult
	{
		
	}

	public class SendSmsResultThereIsActiveDispatch : SendSmsResult
	{
		
	}

    public class SendSmsResultDispatchViaSmsIsFalse : SendSmsResult { }

    public class SendSmsResultAlreadyHasDispatches : SendSmsResult { }

    public class SendSmsResultDidntAffectActiveDispatch : SendSmsResult { }

    public class SendSmsResultPreferredFormatIsNotSms : SendSmsResult { }

	public class SendSmsResultError : SendSmsResult
	{
		public SendSmsResultError(string errorMessage)
		{
			ErrorMessage = errorMessage;
		}
		public string ErrorMessage { get; set; }
	}
}
