#region COPYRIGHT
// File:     Defaults.cs
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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machete.Data.Identity;
using Machete.Domain;
using Machete.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Machete.Web.Helpers
{
    public interface IDefaults
    {
        int daysDefault { get; }
        double hourlyWageDefault { get; }
        int hoursDefault { get; }
        int skillLevelDefault { get; }

        string byID(int? ID);
        string byID(int ID);
        int byKeys(string category, string key);
        SelectList configCategories();
        SelectList days();
        string getBool(bool? val);
        string getBool(bool val);
        int getDefaultID(string type);
        int getDefaultSkillHours();
        double getDefaultSkillWage();
        List<SelectListItemEmail> getEmailTemplates();
        List<SelectListItemEx> getEmployerSkill();
        List<SelectListEmployerSkills> getOnlineEmployerSkill();
        SelectList getSelectList(string type);
        List<SelectListItemEx> getSkill(bool specializedOnly);
        SelectList getTransportationMethodList();
        SelectList hours();
        SelectList skillLevels();
        List<SelectListItem> yesnoSelectList();
        Task<IEnumerable<string>> getTeachers();
        string getConfig(string key);
    }

    public class Defaults : IDefaults
    {
        public int hoursDefault { get { return getDefaultSkillHours(); } }
        public int daysDefault { get { return 1;  } }
        public int skillLevelDefault { get { return 1; } }
        public double hourlyWageDefault { get { return getDefaultSkillWage(); } }
        public SelectList hours() { return hoursNum; }
        public SelectList days() { return daysNum; }
        public SelectList skillLevels() { return skillLevelNum; }
        public SelectList configCategories() { return categories; }
        //
        private SelectList hoursNum { get; set; }
        private SelectList daysNum { get; set; }
        private SelectList categories { get; set; }
        private SelectList skillLevelNum { get; set; }
        private List<SelectListItem> yesnoEN { get; set; }
        private List<SelectListItem> yesnoES { get; set; }
        private ILookupService lServ;
        private IConfigService cfgServ;
        private ITransportProvidersService tpServ;

        private UserManager<MacheteUser> _userManager;
        private RoleManager<MacheteRole> _roleManager;

        //
        // Initialize once to prevent re-querying DB
        //
        public Defaults(
            ILookupService lServ, 
            IConfigService cfg, 
            ITransportProvidersService tpServ,             
            UserManager<MacheteUser> userManager,
            RoleManager<MacheteRole> roleManager
        )
        {
            cfgServ = cfg;
            this.lServ = lServ;
            this.tpServ = tpServ;
            hoursNum = new SelectList(new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16" }
                .Select(x => new LookupNumber { Value = x, Text = x }),
                "Value", "Text", "7"
                );
            daysNum = new SelectList(new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14" }
                .Select(x => new LookupNumber { Value = x, Text = x }),
                "Value", "Text", "7"
                );            
            skillLevelNum = new SelectList(new[] { "0", "1", "2", "3" }
                .Select(x => new SelectListItem { Value = x, Text = x }),
                "Value", "Text", "0");
            categories = new SelectList(
                new[] {
                    LCategory.maritalstatus,
                    LCategory.race,
                    LCategory.neighborhood,
                    LCategory.gender,
                    LCategory.transportmethod,
                    LCategory.transportTransactType,
                    LCategory.countryoforigin,
                    LCategory.activityName, 
                    LCategory.activityType,
                    LCategory.eventtype,
                    LCategory.orderstatus,
                    LCategory.emplrreference, 
                    LCategory.worktype,
                    LCategory.memberstatus,
                    LCategory.skill,
                    LCategory.emailstatus,
                    LCategory.emailTemplate,
                    LCategory.housingType,
                    LCategory.vehicleTypeID,
                    LCategory.workerRating,
                    LCategory.incomeSourceID,
                    LCategory.usBornChildren,
                    LCategory.educationLevel,
                    LCategory.farmLabor,
                    LCategory.training,
                    LCategory.income
                }
                .Select(x => new SelectListItem { Value = x, Text = x }),
                "Value", "Text", LCategory.activityName);

            yesnoEN = new List<SelectListItem>();
            yesnoEN.Add(new SelectListItem { Selected = false, Text = "No", Value = "false" });
            yesnoEN.Add(new SelectListItem { Selected = false, Text = "Yes", Value = "true" });
            yesnoES = new List<SelectListItem>();
            yesnoES.Add(new SelectListItem { Selected = false, Text = "No", Value = "false" });
            yesnoES.Add(new SelectListItem { Selected = false, Text = "Sí", Value = "true" });

            _userManager = userManager;
            _roleManager = roleManager;
        }
        public CultureInfo getCI()
        {
            return Thread.CurrentThread.CurrentUICulture;
        }
        public string getCIString()
        {
            return getCI().TwoLetterISOLanguageName.ToUpperInvariant();
        }
        //TODO: Defaults.yesno needs to use resource files, not hardcoded values
        public List<SelectListItem> yesnoSelectList()
        {
            if (getCIString() == "ES") return yesnoES;
            return yesnoEN;  //defaults to English
        }
        //
        // Get the Id string for a given lookup number
        public string byID(int ID)
        {
            return lServ.textByID(ID, getCIString());
        }
        public string byID(int? ID)
        {
            return ID == null ? null : byID((int)ID);
        }
        //
        // Get the ID number for a given lookup string
        public int byKeys(string category, string key)
        {
            return lServ.GetByKey(category, key).ID;
        }
        //
        // create multi-lingual yes/no strings
        public string getBool(bool val)
        {
            if (val) return 
                string.Equals(Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant(), "es")
                ? "sí"
                : "yes";
            
            return "no";
        }
        public string getBool(bool? val)
        {            
            return getBool(val ?? false);
        }

        /// <summary>
        /// Gets the default ID for the group
        /// </summary>
        /// <returns></returns>
        public int getDefaultID(string type)
        {
            var defaultID = lServ.GetMany(s => s.selected && s.category == type).ToList();
            var count = defaultID.Count;
            if (count > 0) {
                return defaultID.First().ID;
            } else {
                return count;
            }
        }

        public double getDefaultSkillWage()
        {
            var wage = 0.0;
            var defaultWage = lServ.GetMany(s => s.selected && s.category == LCategory.skill && s.active).ToList();
            var count = defaultWage.Count();
            if (count > 0)
            {
                return defaultWage.First().wage ?? 0.0;
            } else {
                return wage;
            }
        }

        public int getDefaultSkillHours()
        {
            var lookupHours = lServ.GetMany(s => s.selected && s.category == LCategory.skill && s.active).ToList();
            var count = lookupHours.Count;
            if (count > 0) {
                return lookupHours.First().minHour ?? 0;
            } else { return 0; }
        }

        public string getConfig(string key)
        {
            return cfgServ.getConfig(key);
        }

        public async Task<IEnumerable<string>> getTeachers()
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            var result = teachers.Select(teach => $"{teach.UserName}"); // TODO people are increasingly using emails; this creates a confusing interface
            return result;
        }
        /// <summary>
        /// Get the SelectList for the specified lookup/category type.
        /// </summary>
        /// <param name="type">The type of the SelectList to be retrieved</param>
        /// <returns></returns>
        public SelectList getSelectList(string type)
        {
            var locale = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant();
            string field;
            if (locale == "ES") field = "text_ES";
            else field = "text_EN";
            var list = new SelectList(lServ.GetMany(s => s.category == type && s.active),
                "ID",
                field,
                getDefaultID(type));
            if (list == null) throw new NullReferenceException("getSelectList() returned no lookups in Default.cs");
            return list;
        }

        /// <summary>
        /// Get the SelectList for the group
        /// </summary>
        /// <param name="locale"></param>
        /// <returns></returns>
        public SelectList getTransportationMethodList()
        {
            var locale = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant();
            string field;
            SelectList list;
            if (locale == "ES") field = "text_ES";
            else field = "text_EN";
            // NOTE: transportation methods hard-coded to support Casa Latina
            var defaultTP = tpServ.GetMany(a => a.defaultAttribute).FirstOrDefault().ID;
            list = new SelectList(tpServ.GetMany(a => a.active),
                                    "ID",
                                    field,
                                    defaultTP);
            if (list == null) throw new NullReferenceException("Get returned no lookups, Defaults.cs getTransportationMethodList()");
            return list;
        }

        public List<SelectListItemEmail> getEmailTemplates()
        {
            var locale = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant();
            IEnumerable<Lookup> prelist = lServ.GetMany(s => s.category == LCategory.emailTemplate);
            return new List<SelectListItemEmail>(prelist
                .Select(x => new SelectListItemEmail
                {
                    Selected = x.selected,
                    Value = Convert.ToString(x.ID),
                    Text = locale == "es" ? x.text_ES : x.text_EN,
                    template = x.emailTemplate
                }));
        }

        /// <summary>
        /// get the List of skills. used in Worker.cshtml & WorkAssignment.cshtml
        /// </summary>
        /// <param name="specializedOnly">only return specialized entries</param>
        /// <returns></returns>
        public List<SelectListItemEx> getSkill(bool specializedOnly)
        {
            var locale = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant();
            IEnumerable<Lookup> prelist = lServ.GetMany(s => s.category == LCategory.skill && s.active);
            Func<Lookup, string> textFunc; //anon function
            if (prelist == null) throw new ArgumentNullException("No skills returned");
            if (specializedOnly)
            {
                //TODO: Selection of ES/EN not scalable on i18n. Kludge.
                textFunc = (ll => "[" + ll.ltrCode + ll.level + "] " + (locale == "es" ? ll.text_ES : ll.text_EN));
                Func<Lookup, string> sortFunc = (ll => locale == "es" ? ll.text_ES : ll.text_EN); //created new sortFunc to sort only by skill text and not by concatenated ltrCode + skills 
                prelist = prelist.Where(s => s.speciality).OrderBy(s => sortFunc(s)); //LINQ & FUNC
            }
            else
            {
                textFunc = (ll => locale == "es" ? ll.text_ES : ll.text_EN);           
                prelist = prelist.OrderBy(s => textFunc(s));
            }
            return new List<SelectListItemEx>(prelist
                    .Select(x => new SelectListItemEx
                    {
                        Selected = x.selected,
                        Value = Convert.ToString(x.ID),
                        Text = textFunc(x),
                        wage = Convert.ToString(x.wage),
                        minHour = Convert.ToString(x.minHour),
                        fixedJob = Convert.ToString(x.fixedJob)
                    }));
        }

        /// <summary>
        /// get the List of skills to present to Employer in Employer online interface
        /// </summary>
        /// <returns>List of skills</returns>
        public List<SelectListItemEx> getEmployerSkill()
        {
            var locale = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant();
            IEnumerable<Lookup> prelist = lServ.GetMany(s => s.category == LCategory.skill && s.active);
            Func<Lookup, string> textFunc; //anon function
            if (prelist == null) throw new ArgumentNullException("No skills returned");
 
            //TODO: Selection of ES/EN not scalable on i18n. Kludge.
            textFunc = (ll => "[" + ll.ltrCode + ll.level + "] " + (locale == "es" ? ll.text_ES : ll.text_EN));
            Func<Lookup, string> sortFunc = (ll => locale == "es" ? ll.text_ES : ll.text_EN); //created new sortFunc to sort only by skill text and not by concatenated ltrCode + skills 
            prelist = prelist.Where(s => s.speciality).OrderBy(s => sortFunc(s)); //LINQ & FUNC
            // TODO: (above) filter by employerView (not speciality)
            // TODO: return typeOfWorkID & description
            return new List<SelectListItemEx>(prelist
                    .Select(x => new SelectListItemEx
                    {
                        Selected = x.selected,
                        Value = Convert.ToString(x.ID),
                        Text = textFunc(x),
                        wage = Convert.ToString(x.wage),
                        minHour = Convert.ToString(x.minHour),
                        fixedJob = Convert.ToString(x.fixedJob)
                    }));
        }

        /// <summary>
        /// get the List of skills to present to Employer in Employer online interface
        /// </summary>
        /// <returns>List of skills</returns>
        public List<SelectListEmployerSkills> getOnlineEmployerSkill()
        {
            Func<Lookup, string> textFunc; //anon function
            var locale = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant();

            //TODO: Selection of ES/EN not scalable on i18n. Kludge.
            textFunc = (ll => (locale == "es" ? ll.text_ES : ll.text_EN));
            Func<Lookup, string> sortFunc = (ll => locale == "es" ? ll.text_ES : ll.text_EN); //created new sortFunc to sort only by skill text and not by concatenated ltrCode + skills 

            IEnumerable<Lookup> prelist = lServ.GetMany(s => s.category == LCategory.skill && s.active)
                                                .OrderBy(s => sortFunc(s)); //LINQ & FUNC
            if (prelist == null) throw new NullReferenceException("No skills returned; Defaults.cs, getOnlineEmployerSkill()");

            return new List<SelectListEmployerSkills>(prelist
                    .Select(x => new SelectListEmployerSkills
                    {
                        Selected = x.selected,
                        Value = Convert.ToString(x.ID),
                        Text = textFunc(x),
                        wage = x.wage == null ? 15 : x.wage.Value,
                        minHour = x.minHour == null ? 1 : x.minHour.Value,
                        ID = x.ID,
                        typeOfWorkID = x.typeOfWorkID == null ? 0 : x.typeOfWorkID.Value,
                        skillDescriptionEs = x.skillDescriptionEs,
                        skillDescriptionEn = x.skillDescriptionEn
                    }));
        }

    }

    public class LookupNumber
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
}
