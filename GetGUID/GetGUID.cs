using System;

using System.Activities;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace SPandCRM
{
    public class GetGUID : CodeActivity
    {
        [Input("Record Dynamic Url")]
        [RequiredArgument]
        public InArgument<string> RecordUrl { get; set; }

        [Output("Record Guid")]
        public OutArgument<string> RecordGuid { get; set; }
        
        protected override void Execute(CodeActivityContext executionContext)
        {
            // Instantiate the tracing service
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();

            if (tracingService == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve tracing service.");
            }

            tracingService.Trace("Entered GetRecordGuid.Execute(), Activity Instance Id: {0}, Workflow Instance Id: {1}",
                executionContext.ActivityInstanceId,
                executionContext.WorkflowInstanceId);

            // Create the context 
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();

            if (context == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve workflow context.");
            }

            tracingService.Trace("GetRecordGuid.Execute(), Correlation Id: {0}, Initiating User: {1}",
                context.CorrelationId,
                context.InitiatingUserId);

            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                var entityReference = new DynamicUrlParser(RecordUrl.Get<string>(executionContext));

                RecordGuid.Set(executionContext, entityReference.Id.ToString());
            }
            catch (FaultException<OrganizationServiceFault> e)
            {
                tracingService.Trace("Exception: {0}", e.ToString());

                // Handle the exception. 
                throw;
            }

            tracingService.Trace("Exiting GetRecordGuid.Execute(), Correlation Id: {0}", context.CorrelationId);
        }
        
    }
}
