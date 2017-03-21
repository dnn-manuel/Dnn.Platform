﻿using System.Collections.Generic;
using Dnn.ExportImport.Components.Dto.Jobs;
using Dnn.ExportImport.Components.Dto.Pages;
using Dnn.ExportImport.Components.Entities;

namespace Dnn.ExportImport.Components.Interfaces
{
    public interface IEntitiesController
    {
        ExportImportJob GetFirstActiveJob();
        ExportImportJob GetJobById(int jobId);
        IList<ExportImportJobLog> GetJobSummaryLog(int jobId);
        IList<ExportImportJobLog> GetJobFullLog(int jobId);
        AllJobsHeader GetAllJobsHeader(int? portalId);
        IList<ExportImportJob> GetAllJobs(int? portalId, int? pageSize, int? pageIndex);
        void UpdateJobStatus(ExportImportJob job);
        void SetJobCancelled(ExportImportJob job);
        void RemoveJob(ExportImportJob job);
        IList<ExportImportChekpoint> GetJobChekpoints(int jobId);
        void UpdateJobChekpoint(ExportImportChekpoint checkpoint);
        IList<ShortTabInfo> GetPortalTabs(int portalId);
    }
}
