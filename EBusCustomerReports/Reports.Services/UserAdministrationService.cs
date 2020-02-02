using Reports.Services.AdministrationDB;
using Reports.Services.Models.UserAdministration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Reports.Services
{
    public class UserAdministrationService : BaseServices
    {
        private EBusReportUserAdministrationContext context = new EBusReportUserAdministrationContext();
        public IEnumerable<SelectListItem> GetCompanies()
        {
            List<SelectListItem> result = new List<SelectListItem>
            {
                new SelectListItem() { Text = "-Select Company-", Value = "0", Selected = true }
            };
            result.AddRange(context.Companies.Select(x => new SelectListItem() { Text = x.CompanyName, Value = x.ID.ToString() }));
            return result.ToList();
        }

        public IEnumerable<SelectListItem> GetApplicationRoles(int roleID)
        {
            List<SelectListItem> result = new List<SelectListItem>
            {
                new SelectListItem() { Text = "-Select Role-", Value = "0", Selected = true }
            };
            result.AddRange(context.ApplicationRoles.Where(x => x.ID >= roleID).Select(x => new SelectListItem() { Text = x.RoleDiscription, Value = x.ID.ToString() }));
            return result.ToList();
        }

        public IEnumerable<AdministrationDB.ApplicationMenu> GetApplicationMenu()
        {
            List<AdministrationDB.ApplicationMenu> result = new List<AdministrationDB.ApplicationMenu>();
            result = context.ApplicationMenus.Include("ApplicationScreens.ApplicationFunctionalities").ToList();
            return result.ToList();
        }

        public IEnumerable<UserInformation> GetUsers(string companyID, string roleID, string userName)
        {
            List<UserInformation> result = new List<UserInformation>();

            try
            {
                result = context.UserInfoes.Include("Company").Include("ApplicationRole").Where(x =>
                (companyID.Equals(string.Empty) || companyID.Equals("0") || x.CompanyID.ToString().Equals(companyID)) &&
                (roleID.Equals(string.Empty) || roleID.Equals("0") || x.RoleID.ToString().Equals(roleID)) &&
                (userName.Equals(string.Empty) || x.UserName.Contains(userName))).ToList().
                Select(x => new UserInformation()
                {
                    UserName = x.UserName,
                    ID = x.ID,
                    Password = x.Password,
                    Company = x.Company.CompanyName,
                    Role = x.ApplicationRole.RoleDiscription,
                    RoleID = x.ApplicationRole.ID,
                    AccessCodes = x.AccessCodes,
                    WarningDate = x.WarningDate.HasValue ? x.WarningDate.Value.ToString("dd-MM-yyyy") : "",
                    LastDate = x.LastDate.HasValue ? x.LastDate.Value.ToString("dd-MM-yyyy") : "",
                }).ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return result.ToList();
        }

        public UserInformation ValidateUser(string userName, string password)
        {
            UserInformation result = new UserInformation();
            try
            {
                result = context.UserInfoes.Include("Company").Include("ApplicationRole")
                    .Where(x => x.UserName.Equals(userName) && x.Password.Equals(password)).ToList().
                Select(x => new UserInformation()
                {
                    UserName = x.UserName,
                    ID = x.ID,
                    Password = x.Password,
                    Company = x.Company.CompanyName,
                    ConnectionKey = x.Company.ConnectionKey,
                    Role = x.ApplicationRole.RoleDiscription,
                    RoleID = x.RoleID,
                    AccessCodes = x.AccessCodes,
                    WarningDate = x.WarningDate.HasValue ? x.WarningDate.Value.ToShortDateString() : DateTime.Now.AddMonths(1).ToShortDateString(),
                    LastDate = x.LastDate.HasValue ? x.LastDate.Value.ToShortDateString() : DateTime.Now.AddMonths(2).ToShortDateString()
                }).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }

        public UserInformation GetUserInformation(string recordID)
        {
            UserInformation result = new UserInformation();
            try
            {
                result = context.UserInfoes.Where(x => x.ID.ToString().Equals(recordID)).
                Select(x => new UserInformation()
                {
                    UserName = x.UserName,
                    Password = x.Password,
                    ID = x.ID,
                    CompanyID = x.CompanyID,
                    RoleID = x.RoleID,
                    AccessCodes = x.AccessCodes,
                    WarningDate = x.WarningDate.HasValue ? x.WarningDate.Value.ToShortDateString() : DateTime.Now.AddMonths(1).ToShortDateString(),
                    LastDate = x.LastDate.HasValue ? x.LastDate.Value.ToShortDateString() : DateTime.Now.AddMonths(2).ToShortDateString()
                }).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }
        public int InsertOrUpdateUserInfo(UserInformation userInformation)
        {
            string[] warnDate = string.IsNullOrEmpty(userInformation.WarningDate) ? "".Split('-') : userInformation.WarningDate.Split('-');
            string[] lstDate = string.IsNullOrEmpty(userInformation.LastDate) ? "".Split('-') : userInformation.LastDate.Split('-');
            Nullable<DateTime> date = null;
            int Status = 1;
            try
            {
                if (userInformation.ID <= 0)
                {
                    List<UserInfo> item = context.UserInfoes.Where(x => x.ID.Equals(userInformation.ID)).ToList();
                    if (item != null && item.Any())
                    {
                        return -1;
                    }

                    context.UserInfoes.Add(new UserInfo()
                    {
                        CompanyID = userInformation.CompanyID,
                        RoleID = userInformation.RoleID,
                        UserName = userInformation.UserName,
                        Password = userInformation.Password,
                        AccessCodes = userInformation.AccessCodes,
                        WarningDate = warnDate.Length > 1 ? new DateTime(Convert.ToInt32(warnDate[2]), Convert.ToInt32(warnDate[1]), Convert.ToInt32(warnDate[0])) : date,
                        LastDate = lstDate.Length > 1 ? new DateTime(Convert.ToInt32(lstDate[2]), Convert.ToInt32(lstDate[1]), Convert.ToInt32(lstDate[0])) : date,
                        CreatedBy = "System",
                        CreatedOn = DateTime.Now,
                        ModifiedBy = "System",
                        ModifiedOn = DateTime.Now,
                        Status = true
                    });
                }
                else
                {
                    UserInfo item = context.UserInfoes.Where(x => x.ID.Equals(userInformation.ID)).FirstOrDefault();
                    item.CompanyID = userInformation.CompanyID;
                    item.RoleID = userInformation.RoleID;
                    item.UserName = userInformation.UserName;
                    item.Password = userInformation.Password;
                    item.AccessCodes = userInformation.AccessCodes;
                    item.WarningDate = warnDate.Length > 1 ? new DateTime(Convert.ToInt32(warnDate[2]), Convert.ToInt32(warnDate[1]), Convert.ToInt32(warnDate[0])) : date;
                    item.LastDate = lstDate.Length > 1 ? new DateTime(Convert.ToInt32(lstDate[2]), Convert.ToInt32(lstDate[1]), Convert.ToInt32(lstDate[0])) : date;
                }
                context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
            return Status;
        }

        public int DeleteUser(string userID)
        {
            int Status = 1;
            try
            {
                context.UserInfoes.Remove(context.UserInfoes.Where(x => x.ID.ToString().Equals(userID)).FirstOrDefault());
                context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
            return Status;
        }
    }
}
