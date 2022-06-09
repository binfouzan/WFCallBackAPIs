using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OptimaJet.Workflow.Core.Model;

namespace WFCallBackApi.Controllers
{
    public class WfQueryRule
    {
        public string? name { get; set; }
        public string? identityId { get; set; }
        public string? parameter { get; set; }
        public object? processInstance { get; set; }
    }

    public class WfQuery
    {
        public string? name { get; set; }
        public string? parameter { get; set; }
        public object? processInstance { get; set; }
    }



    public class CallBackResponse
    {
        public Boolean success { get; set; }
        public object? data { get; set; }
    }

    public class user
    {
        public string? name { get; set; }
        public string? id { get; set; }
        public string? role { get; set; }
    }

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CallBackApisController : ControllerBase
    {

        private List<user> users = new List<user>();

        public CallBackApisController()
        {
            users.Add(new user { id = "user1", name = "user1", role = "user" });
            users.Add(new user { id = "user2", name = "user2", role = "teamleader" });
            users.Add(new user { id = "user3", name = "user3", role = "manager" });
            users.Add(new user { id = "user4", name = "user3", role = "dmd" });
        }

        [HttpGet]
        public IActionResult getactions(string schemeCode)
        {
            var actions = new List<string> { "Action4", "Action5", "Action6" };
            var res = new CallBackResponse { success = true, data = actions };

            return Ok(res);
        }

        [HttpPost]
        public IActionResult executeaction([FromBody] WfQuery wfQuery)
        {
            return Ok(new CallBackResponse());
        }

        [HttpGet]
        public IActionResult getcondetions(string schemeCode)
        {
            var conditions = new List<string> { "IsTeamleader", "IsManager","IsUser"};
            var res = new CallBackResponse { success = true, data = conditions };
            return Ok(res);
        }

        [HttpPost]
        public IActionResult executecondition([FromBody] WfQuery wfQuery)
        {
            var processInstance = JsonConvert.DeserializeObject<dynamic>(wfQuery.processInstance.ToString());
            string creatorId = processInstance.ProcessParameters.creatorIdentity;
            if (wfQuery.name == "IsUser")
            {
                var isInRole = IsInRole(creatorId, "user");
                if (isInRole)
                {
                    return Ok(new CallBackResponse { success = true, data = true });
                }
                else
                {
                    return Ok(new CallBackResponse { success = true, data = false });
                }

            }
            else if(wfQuery.name == "IsTeamleader")
            {
                var isInRole = IsInRole(creatorId, "teamleader");
                if (isInRole)
                {
                    return Ok(new CallBackResponse { success = true, data = true });
                }
                else
                {
                    return Ok(new CallBackResponse { success = true, data = false });
                }
            }
            else if(wfQuery.name == "IsManager")
            {
                var isInRole = IsInRole(creatorId, "manager");
                if (isInRole)
                {
                    return Ok(new CallBackResponse { success = true, data = true });
                }
                else
                {
                    return Ok(new CallBackResponse { success = true, data = false });
                }
            }
            else
            {
                return Ok(new CallBackResponse { success = true, data = false });
            }
            
        }

        [HttpGet]
        public IActionResult getrules(string schemeCode)
        {
            var rules = new List<string> { "CheckRoleCallBack" };
            return Ok(new CallBackResponse { success = true, data = rules });
        }

        [HttpPost]
        public IActionResult checkrule([FromBody] WfQueryRule wfQueryRule)
        {
            if(wfQueryRule.name == "CheckRoleCallBack")
            {
                string? roleName = wfQueryRule.parameter;
                string? userId = wfQueryRule.identityId;
                var res = IsInRole(userId, roleName);
                return Ok(new CallBackResponse { success = true, data = res });
            }
            return Ok(new CallBackResponse { success=true, data=false });
        }

        [HttpPost]
        public IActionResult getidentites([FromBody] WfQuery wfQuery)
        {

            var temp = JsonConvert.DeserializeObject<dynamic>(wfQuery.processInstance.ToString());
            //var id = temp.Id;

            var users = UsersInRole(wfQuery.parameter);
            return Ok(new CallBackResponse { success=true, data= users });
        }

        [HttpPost]
        public IActionResult processstatuschanged() { return Ok(); }




        private List<string?> UsersInRole(string roleName)
        {
            return users.Where(x => x.role == roleName).Select(x => x.name).ToList();
        }

        private bool IsInRole(string userName,string roleName)
        {
            var res = users.Where(x => x.id == userName).Where(x => x.role == roleName).Any();
            return (res);
        }
    }
}
