﻿using AutoMapper;
using Machete.Domain;
using Machete.Service;
using DTO = Machete.Service.DTO;
using Machete.Web.Helpers;
using Machete.Web.Resources;
using Machete.Web.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Machete.Web
{
    public class MapperConfig
    {
        MapperConfiguration cfg;
        Mapper map;
        public MapperConfig()
        {
            cfg = new MapperConfiguration(c => {
                c.CreateMap<jQueryDataTableParam, viewOptions>()
                    .ForMember(vo => vo.CI, opt => opt.Ignore())
                    .ForMember(vo => vo.authenticated, opt => opt.Ignore())
                    .ForMember(vo => vo.personID, opt => opt.MapFrom(dt => dt.personID ?? 0))
                    .ForMember(vo => vo.emailID, opt => opt.MapFrom(dt => string.IsNullOrEmpty(dt.searchColName("emailID")) ? null : (int?)Convert.ToInt32(dt.searchColName("emailID"))))
                    .ForMember(vo => vo.onlineSource, opt => opt.MapFrom(dt => string.IsNullOrEmpty(dt.searchColName("onlineSource")) ? null : dt.searchColName("onlineSource")))
                    .ForMember(vo => vo.status, opt => opt.MapFrom(dt => string.IsNullOrEmpty(dt.searchColName("status")) ? null : (int?)Convert.ToInt32(dt.searchColName("status"))))
                    .ForMember(vo => vo.EmployerID, opt => opt.MapFrom(dt => string.IsNullOrEmpty(dt.searchColName("EID")) ? null : (int?)Convert.ToInt32(dt.searchColName("EID"))))
                    .ForMember(vo => vo.sortColName, opt => opt.MapFrom(dt => dt.sortColName()))
                    .ForMember(vo => vo.dwccardnum, opt => opt.MapFrom(dt => Convert.ToInt32(dt.dwccardnum)))
                    .ForMember(vo => vo.woid, opt => opt.MapFrom(dt => Convert.ToInt32(dt.searchColName("WOID"))))
                    .ForMember(vo => vo.date, opt => opt.MapFrom(dt => dt.todaysdate == null ? null : (DateTime?)DateTime.Parse(dt.todaysdate)))
                    .ForMember(vo => vo.displayStart, opt => opt.MapFrom(dt => dt.iDisplayStart))
                    .ForMember(vo => vo.displayLength, opt => opt.MapFrom(dt => dt.iDisplayLength))
                    .ForMember(vo => vo.orderDescending, opt => opt.MapFrom(dt => dt.sSortDir_0 == "asc" ? false : true));

                #region WoCombined
                // Splitting Combined into parts
                c.CreateMap<EmployerWoCombined, Domain.Employer>();
                // Splitting Combined into parts
                c.CreateMap<EmployerWoCombined, Domain.WorkOrder>();
                // re-combineing to view model object
                c.CreateMap<Domain.Employer, EmployerWoCombined>();
                // re-combineing to view model object
                c.CreateMap<Domain.WorkOrder, EmployerWoCombined>()
                    .ForMember(wo => wo.wo_city, opt => opt.MapFrom(e => e.city))
                    .ForMember(wo => wo.wo_state, opt => opt.MapFrom(e => e.state))
                    .ForMember(wo => wo.wo_phone, opt => opt.MapFrom(e => e.phone))
                    .ForMember(wo => wo.wo_zipcode, opt => opt.MapFrom(e => e.zipcode));
                #endregion
                c.CreateMap<Email, EmailView>()
                    .ForMember(ev => ev.statusID, opt => opt.MapFrom(e => e.statusID));
                c.CreateMap<EmailView, Email>()
                    .ForMember(e => e.updatedby, opt => opt.Ignore())
                    .ForMember(e => e.createdby, opt => opt.Ignore())
                    .ForMember(e => e.datecreated, opt => opt.Ignore())
                    .ForMember(e => e.dateupdated, opt => opt.Ignore());
                c.CreateMap<Domain.Employer, ViewModel.Employer>()
                    .ForMember(v => v.tabref, opt => opt.MapFrom(d => "/Employer/Edit/" + Convert.ToString(d.ID)))
                    .ForMember(v => v.tablabel, opt => opt.MapFrom(d => d.name))
                    .ForMember(v => v.active, opt => opt.MapFrom(d => Convert.ToString(d.active)))
                    //.ForMember(v => v.EID, opt => opt.MapFrom(d => Convert.ToString(d.ID)))
                    //.ForMember(v => v.recordid, opt => opt.MapFrom(d => Convert.ToString(d.ID)))
                    .ForMember(v => v.dateupdated, opt => opt.MapFrom(d => Convert.ToString(d.dateupdated)))
                    .ForMember(v => v.onlineSource, opt => opt.MapFrom(d => d.onlineSource.ToString()));
                c.CreateMap<Domain.Employer, DTO.EmployerList>();
                c.CreateMap<DTO.EmployerList, ViewModel.EmployerList>()
                    .ForMember(v => v.tabref, opt => opt.MapFrom(d => "/Employer/Edit/" + Convert.ToString(d.ID)))
                    .ForMember(v => v.tablabel, opt => opt.MapFrom(d => d.name))
                    .ForMember(v => v.active, opt => opt.MapFrom(d => Convert.ToString(d.active)))
                    .ForMember(v => v.EID, opt => opt.MapFrom(d => Convert.ToString(d.ID)))
                    .ForMember(v => v.recordid, opt => opt.MapFrom(d => Convert.ToString(d.ID)))
                    .ForMember(v => v.dateupdated, opt => opt.MapFrom(d => Convert.ToString(d.dateupdated)))
                    .ForMember(v => v.onlineSource, opt => opt.MapFrom(d => d.onlineSource.ToString()));
                c.CreateMap<Domain.WorkOrder, ViewModel.WorkOrder>()
                    .ForMember(v => v.tabref, opt => opt.MapFrom(d => "/WorkOrder/Edit/" + Convert.ToString(d.ID)))
                    .ForMember(v => v.tablabel, opt => opt.MapFrom(d =>
                        Machete.Web.Resources.WorkOrders.tabprefix +
                        d.paperOrderNum == null ?
                            System.String.Format("{0,5:D5}", d.ID) :
                            Convert.ToString(d.paperOrderNum)
                        + " @ " + d.workSiteAddress1))
                    .ForMember(v => v.EID, opt => opt.MapFrom(d => Convert.ToString(d.EmployerID)))
                    .ForMember(v => v.WOID, opt => opt.MapFrom(d => System.String.Format("{0,5:D5}", d.paperOrderNum)))
                    .ForMember(v => v.dateTimeofWork, opt => opt.MapFrom(d => Convert.ToString(d.dateTimeofWork)))
                    .ForMember(v => v.dateupdatedstring, opt => opt.MapFrom(d => System.String.Format("{0:MM/dd/yyyy HH:mm:ss}", d.dateupdated)))
                    .ForMember(v => v.onlineSource, opt => opt.MapFrom(d => d.onlineSource ? Shared.True : Shared.False))
                    .ForMember(v => v.recordid, opt => opt.MapFrom(d => Convert.ToString(d.ID)));
                c.CreateMap<Domain.WorkOrder, DTO.WorkOrderList>()
                    .ForMember(v => v.WAcount, opt => opt.MapFrom(d => d.workAssignments.Count()))
                    .ForMember(v => v.emailSentCount, opt => opt.MapFrom(d => d.Emails.Where(e => e.statusID == Email.iSent || e.statusID == Email.iReadyToSend).Count()))
                    .ForMember(v => v.emailErrorCount, opt => opt.MapFrom(d => d.Emails.Where(e => e.statusID == Email.iTransmitError).Count()))
                ;
                c.CreateMap<DTO.WorkOrderList, ViewModel.WorkOrderList>()
                    .ForMember(v => v.tabref, opt => opt.MapFrom(d => "/WorkOrder/Edit/" + Convert.ToString(d.ID)))
                    .ForMember(v => v.tablabel, opt => opt.MapFrom(d => 
                        Machete.Web.Resources.WorkOrders.tabprefix +
                        d.paperOrderNum == null ?
                            System.String.Format("{0,5:D5}", d.ID) :
                            Convert.ToString(d.paperOrderNum)
                        + " @ " + d.workSiteAddress1))
                    .ForMember(v => v.EID, opt => opt.MapFrom(d => Convert.ToString(d.EmployerID)))
                    .ForMember(v => v.WOID, opt => opt.MapFrom(d => System.String.Format("{0,5:D5}", d.paperOrderNum)))
                    .ForMember(v => v.dateTimeofWork, opt => opt.MapFrom(d => Convert.ToString(d.dateTimeofWork)))
                    .ForMember(v => v.dateupdated, opt => opt.MapFrom(d => Convert.ToString(d.dateupdated)))
                    ;


            });
        }


        public IMapper getMapper()
        {
            return map ?? (map = new Mapper(cfg));
        }


    }
}