using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Reports.Services.AdministrationDB;
using Reports.Services.Models.UserAdministration;

namespace Reports.Services
{
    public class UserAdministrationService : BaseServices
    {
        EBusReportUserAdministrationContext context = new EBusReportUserAdministrationContext();
        public IEnumerable<SelectListItem> GetCompanies()
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "-Select Company-", Value = "0", Selected = true });
            result.AddRange(context.Companies.Select(x => new SelectListItem() { Text = x.CompanyName, Value = x.ID.ToString() }));
            return result.ToList();
        }

        public IEnumerable<SelectListItem> GetApplicationRoles(int roleID)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Text = "-Select Role-", Value = "0", Selected = true });
            result.AddRange(context.ApplicationRoles.Where(x => x.ID >= roleID).Select(x => new SelectListItem() { Text = x.RoleDiscription, Value = x.ID.ToString() }));
            return result.ToList();
        }

        public IEnumerable<AdministrationDB.ApplicationMenu> GetApplicationMenu()
        {
            var result = new List<AdministrationDB.ApplicationMenu>();
            result = context.ApplicationMenus.Include("ApplicationScreens.ApplicationFunctionalities").ToList();
            return result.ToList();
        }

        public IEnumerable<UserInformation> GetUsers(string companyID, string roleID, string userName)
        {
            var result = new List<UserInformation>();

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
                    AccessCodes = x.AccessCodes
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
            var result = new UserInformation();
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
                    AccessCodes = x.AccessCodes
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
            var result = new UserInformation();
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
                    AccessCodes = x.AccessCodes
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
            var Status = 1;
            try
            {
                if (userInformation.ID <= 0)
                {
                    var item = context.UserInfoes.Where(x => x.ID.Equals(userInformation.ID)).ToList();
                    if (item != null && item.Any()) return -1;
                    context.UserInfoes.Add(new UserInfo()
                    {
                        CompanyID = userInformation.CompanyID,
                        RoleID = userInformation.RoleID,
                        UserName = userInformation.UserName,
                        Password = userInformation.Password,
                        AccessCodes = userInformation.AccessCodes,
                        CreatedBy = "System",
                        CreatedOn = DateTime.Now,
                        ModifiedBy = "System",
                        ModifiedOn = DateTime.Now,
                        Status = true
                    });
                }
                else
                {
                    var item = context.UserInfoes.Where(x => x.ID.Equals(userInformation.ID)).FirstOrDefault();
                    item.CompanyID = userInformation.CompanyID;
                    item.RoleID = userInformation.RoleID;
                    item.UserName = userInformation.UserName;
                    item.Password = userInformation.Password;
                    item.AccessCodes = userInformation.AccessCodes;
                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
            return Status;
        }

        public int DeleteUser(string userID)
        {
            var Status = 1;
            try
            {
                context.UserInfoes.Remove(context.UserInfoes.Where(x => x.ID.ToString().Equals(userID)).FirstOrDefault());
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
            return Status;
        }
    }
}
