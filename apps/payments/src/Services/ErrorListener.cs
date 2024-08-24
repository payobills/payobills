using HotChocolate.Resolvers;
using Microsoft.Extensions.Logging;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Execution;

namespace Payobills.Payments.Services
{ public class ErrorLoggingDiagnosticsEventListener : ExecutionDiagnosticEventListener
    {
        private readonly ILogger<ErrorLoggingDiagnosticsEventListener> log;

        public ErrorLoggingDiagnosticsEventListener(
            ILogger<ErrorLoggingDiagnosticsEventListener> log)
        {
            this.log = log;
        }

        public override void ResolverError(
            IMiddlewareContext context,
            IError error)
        {
            log.LogError(error.Exception, error.Message);
        }

        public override void TaskError(
            IExecutionTask task,
            IError error)
        {
            log.LogError(error.Exception, error.Message);
        }

        public override void RequestError(
            IRequestContext context,
            Exception exception)
        {
            log.LogError(exception, "RequestError");
        }

        public override void SubscriptionEventError(
            SubscriptionEventContext context,
            Exception exception)
        {
            log.LogError(exception, "SubscriptionEventError");
        }
    }

}