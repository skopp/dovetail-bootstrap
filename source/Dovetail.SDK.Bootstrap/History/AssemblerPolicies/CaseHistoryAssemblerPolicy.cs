using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.History.Configuration;
using FChoice.Foundation;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.Bootstrap.History.AssemblerPolicies
{
    public class CaseHistoryAssemblerPolicy : IHistoryAssemblerPolicy
    {
        private readonly IClarifySession _session;
        private readonly HistoryBuilder _historyBuilder;
        private readonly HistorySettings _historySettings;

        public CaseHistoryAssemblerPolicy(IClarifySession session, HistoryBuilder historyBuilder, HistorySettings historySettings)
        {
            _session = session;
            _historyBuilder = historyBuilder;
            _historySettings = historySettings;
        }

        public bool Handles(WorkflowObject workflowObject)
        {
			return workflowObject.Type == WorkflowObject.Case;
        }

        public IEnumerable<HistoryItem> BuildHistory(WorkflowObject workflowObject, Filter actEntryFilter)
        {
			if(!_historySettings.MergeCaseHistoryChildSubcases)
			{
				return _historyBuilder.Build(workflowObject, actEntryFilter);
			}

            var subcaseIds = GetSubcaseIds(workflowObject);
            
            var caseHistory = _historyBuilder.Build(workflowObject, actEntryFilter);

            var subcaseHistories = subcaseIds.Select(id =>
                                                         {
                                                             var subcaseWorkflowObject = new WorkflowObject { Type = WorkflowObject.Subcase, Id = id, IsChild = true };
                                                             return _historyBuilder.Build(subcaseWorkflowObject, actEntryFilter);
                                                         });

            var results = subcaseHistories.SelectMany(result => result).Concat(caseHistory);

            return results.OrderByDescending(r => r.When);
        }

    	public IEnumerable<HistoryItem> BuildHistories(string type, string[] ids, Filter actEntryFilter)
    	{
    		return ids.SelectMany(id =>
    			{
    				var workflowObject = WorkflowObject.Create(type, id);
    				return BuildHistory(workflowObject, actEntryFilter);
    			});
    	}

    	private IEnumerable<string> GetSubcaseIds(WorkflowObject workflowObject)
        {
            var clarifyDataSet = _session.CreateDataSet();
            var caseGeneric = clarifyDataSet.CreateGenericWithFields("case");
            caseGeneric.AppendFilter("id_number", StringOps.Equals, workflowObject.Id);

            var subcaseGeneric = caseGeneric.TraverseWithFields("case2subcase", "id_number");

            caseGeneric.Query();

            return subcaseGeneric.Count > 0 ? subcaseGeneric.DataRows().Select(s => s.AsString("id_number")) : new string[0];
        }
    }
}