#region COPYRIGHT
// File:     WorkOrderController.cs
// Author:   Savage Learning, LLC.
// Created:  2012/06/17 
// License:  GPL v3
// Project:  Machete.Web
// Contact:  savagelearning
// 
// Copyright 2011 Savage Learning, LLC., all rights reserved.
// 
// This source file is free software, under either the GPL v3 license or a
// BSD style license, as supplied with this software.
// 
// This source file is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the license files for details.
//  
// For details please refer to: 
// http://www.savagelearning.com/ 
//    or
// http://www.github.com/jcii/machete/
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Machete.Data.Tenancy;
using Machete.Domain;
using Machete.Service;
using Machete.Web.Helpers;
using Machete.Web.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WorkerRequest = Machete.Domain.WorkerRequest;
using WorkOrder = Machete.Domain.WorkOrder;
using WorkOrdersList = Machete.Service.DTO.WorkOrdersList;

namespace Machete.Web.Controllers
{
        public class WorkOrderController : MacheteController
    {
        private readonly IWorkOrderService _woServ;
        private readonly IMapper _map;
        private readonly IDefaults _defaults;
        private readonly IModelBindingAdaptor _adaptor;
        private readonly TimeZoneInfo _clientTimeZoneInfo;
        private readonly TimeZoneInfo _serverTimeZoneInfo;

        /// <summary>
        /// The Work Order controller is responsible for handling all REST actions related to the
        /// creation, modification, processing and retaining of Work Orders created by staff or
        /// employers (hirers/2.0).
        /// </summary>
        /// <param name="woServ">Work Order service</param>
        /// <param name="defaults">Default config values</param>
        /// <param name="map">AutoMapper service</param>
        /// <param name="adaptor"></param>
        /// <param name="tenantService"></param>
        public WorkOrderController(
            IWorkOrderService woServ,
            IDefaults defaults,
            IMapper map,
            IModelBindingAdaptor adaptor,
            ITenantService tenantService
        )
        {
            _woServ = woServ;
            _map = map;
            _adaptor = adaptor;
            _defaults = defaults;
            _clientTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(tenantService.GetCurrentTenant().Timezone);
            _serverTimeZoneInfo = TimeZoneInfo.Local;
        }
        /// <summary>
        /// Initialize controller
        /// </summary>
        /// <param name="requestContext">Request Context</param>
        protected override void Initialize(ActionContext requestContext)
        {
            base.Initialize(requestContext);
            ViewBag.def = _defaults;
        }
        /// <summary>
        /// HTTP GET /WorkOrder/Index
        /// </summary>
        /// <returns>MVC Action Result</returns>
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Provides json grid of wo/wa summaries and their statuses
        /// </summary>
        /// <param name="param">contains parameters for filtering</param>
        /// <returns>JsonResult for DataTables consumption</returns>
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult AjaxSummary(jQueryDataTableParam param)
        {
            // Retrieve WO/WA Summary based on parameters
            dataTableResult<WOWASummary> dtr = 
                _woServ.CombinedSummary(param.sSearch,
                    Request.Query["sSortDir_0"] != "asc",
                    param.iDisplayStart,
                    param.iDisplayLength);
            //
            //return what's left to datatables
            var result = from p in dtr.query
                         select new[] {
                             $"{TimeZoneInfo.ConvertTimeFromUtc(p.date ?? DateTime.UtcNow, _clientTimeZoneInfo):MM/dd/yyyy hh:mm zz}",
                             p.weekday,
                             p.pending_wo > 0 ? p.pending_wo.ToString(): null,
                             p.pending_wa > 0 ? p.pending_wa.ToString(): null,
                             p.active_wo > 0 ? p.active_wo.ToString(): null,
                             p.active_wa > 0 ? p.active_wa.ToString(): null,
                             p.completed_wo > 0 ? p.completed_wo.ToString(): null,
                             p.completed_wa > 0 ? p.completed_wa.ToString(): null,
                             p.cancelled_wo > 0 ? p.cancelled_wo.ToString(): null,
                             p.cancelled_wa > 0 ? p.cancelled_wa.ToString(): null,
                             p.expired_wo > 0 ? p.expired_wo.ToString(): null,
                             p.expired_wa > 0 ? p.expired_wa.ToString(): null
                         };

            return Json(new
            {
                param.sEcho,
                iTotalRecords = dtr.totalCount,
                iTotalDisplayRecords = dtr.filteredCount,
                aaData = result
            });
        }
        // TODO: investigate why the following columns aren't properly sortable: Weekday, Completed Assignment
        // TODO: investigate why the following work orders aren't appearing in the summary results: active & cancelled - they only appear once a WA is created!!!

        /// <summary>
        /// Provides json grid of orders
        /// </summary>
        /// <param name="param">contains parameters for filtering</param>
        /// <returns>JsonResult for DataTables consumption</returns>
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult AjaxHandler(jQueryDataTableParam param)
        {
            var vo = _map.Map<jQueryDataTableParam, viewOptions>(param);
            //Get all the records
            var dataTableResult = _woServ.GetIndexView(vo);

            MapperHelpers.ClientTimeZoneInfo = _clientTimeZoneInfo;
            var result = dataTableResult.query
                .Select(
                    e => _map.Map<WorkOrdersList, ViewModel.WorkOrdersList>(e)
                ).AsEnumerable();
            return Json(new
            {
                param.sEcho,
                iTotalRecords = dataTableResult.totalCount,
                iTotalDisplayRecords = dataTableResult.filteredCount,
                aaData = result
            });
        }
        /// <summary>
        /// GET: /WorkOrder/Create
        /// </summary>
        /// <param name="employerId">Employer ID associated with Work Order (Parent Object)</param>
        /// <returns>MVC Action Result</returns>
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult Create(int employerId)
        {
            var serverNow = DateTime.Now;
            var utcNow = TimeZoneInfo.ConvertTimeToUtc(serverNow, _serverTimeZoneInfo);
            //var clientNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _clientTimeZoneInfo);
            
            var workOrder = new WorkOrder
            {
                EmployerID = employerId,
                dateTimeofWork = utcNow,
                transportMethodID = _defaults.getDefaultID(LCategory.transportmethod),
                typeOfWorkID = _defaults.getDefaultID(LCategory.worktype),
                statusID = _defaults.getDefaultID(LCategory.orderstatus),
                timeFlexible = true
            };

            MapperHelpers.ClientTimeZoneInfo = _clientTimeZoneInfo;
            MapperHelpers.Defaults = _defaults;
            var wo = _map.Map<WorkOrder, ViewModel.WorkOrder>(workOrder);

            ViewBag.workerRequests = new List<SelectListItem>();
            return PartialView("Create", wo);
        }
        /// <summary>
        /// POST: /WorkOrder/Create
        /// </summary>
        /// <param name="wo">WorkOrder to create</param>
        /// <param name="userName">User performing action</param>
        /// <returns>JSON Object representing new Work Order</returns>
        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public async Task<ActionResult> Create(WorkOrder wo, string userName)
        {
            ModelState.ThrowIfInvalid();
            var modelUpdated = await _adaptor.TryUpdateModelAsync(this, wo);
            if (!modelUpdated) return StatusCode(500);

            wo.dateTimeofWork = TimeZoneInfo.ConvertTimeToUtc(wo.dateTimeofWork, _clientTimeZoneInfo);

            var workOrder = _woServ.Create(wo, userName);

            MapperHelpers.ClientTimeZoneInfo = _clientTimeZoneInfo;
            var result = _map.Map<WorkOrder, ViewModel.WorkOrder>(workOrder);
            return Json(new {
                sNewRef = result.tabref,
                sNewLabel = result.tablabel,
                iNewID = result.ID
            });

        }
        /// <summary>
        /// GET: /WorkOrder/Edit/ID
        /// </summary>
        /// <param name="id">WorkOrder ID</param>
        /// <returns>MVC Action Result</returns>
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult Edit(int id)
        {
            // Retrieve Work Order
            WorkOrder workOrder = _woServ.Get(id);
            
            // Retrieve Worker Requests associated with Work Order
            var workerRequests = workOrder.workerRequests;
            var selectListItems = workerRequests?.Select(a => 
                new SelectListItem
                {
                    Value = a.WorkerID.ToString(), 
                    Text = a.workerRequested.dwccardnum.ToString() + ' ' + 
                           a.workerRequested.Person.firstname1 + ' ' + 
                           a.workerRequested.Person.lastname1 
                });
            ViewBag.workerRequests = selectListItems;
            
            MapperHelpers.ClientTimeZoneInfo = _clientTimeZoneInfo;
            MapperHelpers.Defaults = _defaults;
            var m = _map.Map<WorkOrder, ViewModel.WorkOrder>(workOrder);

            return PartialView("Edit", m);
        }
        /// <summary>
        /// POST: /WorkOrder/Edit/ID
        /// </summary>
        /// <param name="id">WorkOrder ID</param>
        /// <param name="userName">UserName performing action</param>
        /// <param name="workerRequestList">List of workers requested</param>
        /// <returns>MVC Action Result</returns>
        //[Bind(Exclude = "workerRequests")]
        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public async Task<ActionResult> Edit(int id, string userName, List<WorkerRequest> workerRequestList)
        {
            ModelState.ThrowIfInvalid();
            
            var workOrder = _woServ.Get(id);
            var modelUpdated = await _adaptor.TryUpdateModelAsync(this, workOrder);
            if (!modelUpdated) return StatusCode(500);

            workOrder.dateTimeofWork = TimeZoneInfo.ConvertTimeToUtc(workOrder.dateTimeofWork, _clientTimeZoneInfo);
            
            _woServ.Save(workOrder, workerRequestList, userName);
            
            return Json(new {
                status = "OK",
                editedID = id
            });
        }
        /// <summary>
        /// GET: /WorkOrder/View/ID
        /// </summary>
        /// <param name="id">WorkOrder ID</param>
        /// <returns>MVC Action Result</returns>
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult View(int id)
        {
            WorkOrder workOrder = _woServ.Get(id);
            
            MapperHelpers.ClientTimeZoneInfo = _clientTimeZoneInfo;
            MapperHelpers.Defaults = _defaults;
            var m = _map.Map<WorkOrder, ViewModel.WorkOrder>(workOrder);

            return View(m);
        }
        /// <summary>
        /// Creates the view for email
        /// </summary>
        /// <param name="id">WorkOrder ID</param>
        /// <returns>MVC Action Result</returns>
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult ViewForEmail(int id)
        {
            WorkOrder workOrder = _woServ.Get(id);
            
            MapperHelpers.ClientTimeZoneInfo = _clientTimeZoneInfo;
            MapperHelpers.Defaults = _defaults;
            var m = _map.Map<WorkOrder, ViewModel.WorkOrder>(workOrder);

            ViewBag.OrganizationName = _defaults.getConfig("OrganizationName");
            return PartialView(m);
        }
        /// <summary>
        /// Creates the view to print all orders for a given day
        /// </summary>
        /// <param name="date">Date to perform action</param>
        /// <param name="assignedOnly">Optional flag: if True, only shows orders that are fully assigned</param>
        /// <returns>MVC Action Result</returns>
        [Authorize(Roles = "Administrator, Manager")]
        public ActionResult GroupView(DateTime date, bool? assignedOnly)
        {
            var utcDate = TimeZoneInfo.ConvertTimeToUtc(date, _clientTimeZoneInfo);

            var v = _woServ.GetActiveOrders(utcDate, assignedOnly ?? false);
            
            MapperHelpers.ClientTimeZoneInfo = _clientTimeZoneInfo;
            MapperHelpers.Defaults = _defaults;
            var view = new WorkOrderGroupPrintView
            {
                orders = v.Select(e => _map.Map<WorkOrder, ViewModel.WorkOrder>(e)).ToList()
            };

            return View(view);
        }
        /// <summary>
        /// Completes all orders for a given day
        /// </summary>
        /// <param name="date">Date to perform action</param>
        /// <param name="userName">UserName performing action</param>
        /// <returns>MVC Action Result</returns>
        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager")]
        public ActionResult CompleteOrders(DateTime date, string userName)
        {
            int count = _woServ.CompleteActiveOrders(date, userName);
            return Json(new
            {
                completedCount = count
            });
        }
        /// <summary>
        /// POST: /WorkOrder/Delete/ID
        /// </summary>
        /// <param name="id">WorkOrder ID</param>
        /// <param name="user">User performing action</param>
        /// <returns>MVC Action Result</returns>
        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager")]
        public ActionResult Delete(int id, string user)
        {
            _woServ.Delete(id, user);
            return Json(new
            {
                status = "OK",
                deletedID = id
            });
        }
        /// <summary>
        /// POST: /WorkOrder/Activate/ID
        /// </summary>
        /// <param name="id">WorkOrder ID</param>
        /// <param name="userName">User performing action</param>
        /// <returns>MVC Action Result</returns>
        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult Activate(int id, string userName)
        {
            var workOrder = _woServ.Get(id);
            // lookup int value for status active
            workOrder.statusID = WorkOrder.iActive;
            _woServ.Save(workOrder, userName);         
            return Json(new
            {
                status = "activated"
            });
        }
    }
}
