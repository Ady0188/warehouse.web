namespace Warehouse.Web.Shared;

public static class ApiEndpoints
{
    private const string BaseApi = "api";

    public static class V1
    {
        public const string Ver = $"{BaseApi}/v1";
        public static class Auth
        {
            private const string Base = $"{Ver}/auth";

            public const string Login = $"{Base}/login";
            public const string Update = $"{Base}/update";
            public const string Reset = $"{Base}/reset";
        }

        public static class Base<T>
        {
            private const string _base = $"{Ver}/{nameof(T)}";

            public const string Delete = $"{_base}/{{id}}";
        }

        public static class Users
        {
            private const string Base = $"{Ver}/users";

            public const string GetAll = Base;
            public const string GetById = $"{Base}/{{Id}}";
            public const string Create = Base;
            public const string Update = $"{Base}/{{Id}}";
            public const string Delete = $"{Base}/{{Id}}";
            public const string Reset = $"{Base}/{{Id}}";
            public const string ChangePassword = $"{Base}/changepass";
        }

        public static class Employees
        {
            private const string Base = $"{Ver}/employees";

            public const string GetAll = Base;
            public const string GetAllCompact = $"{Base}/compact";
            public const string GetById = $"{Base}/{{Id}}";
            public const string Create = Base;
            public const string Update = $"{Base}/{{Id}}";
            public const string Delete = $"{Base}/{{Id}}";
            public const string Reset = $"{Base}/{{Id}}";
        }

        public static class History
        {
            private const string Base = $"{Ver}/histories";

            public const string GetAll = Base;
            public const string GetById = $"{Base}/{{Id}}";
        }

        public static class Stores
        {
            private const string Base = $"{Ver}/stores";

            public const string GetAll = Base;
            public const string GetById = $"{Base}/{{Id}}";
            public const string Create = Base;
            public const string Update = $"{Base}/{{Id}}";
            public const string Delete = $"{Base}/{{Id}}";
        }

        public static class Managers
        {
            private const string Base = $"{Ver}/managers";

            public const string GetAll = Base;
            public const string GetAllCompact = $"{Base}/compact";
            public const string GetAgents = $"{Base}/agents";
            public const string GetById = $"{Base}/{{Id}}";
            public const string Create = Base;
            public const string Update = $"{Base}/{{Id}}";
            public const string Delete = $"{Base}/{{Id}}";
        }

        public static class Agents
        {
            private const string Base = $"{Ver}/agents";

            public const string GetAll = Base;
            public const string GetById = $"{Base}/{{Id}}";
            public const string Create = Base;
            public const string Update = $"{Base}/{{Id}}";
            public const string Delete = $"{Base}/{{Id}}";
            public const string Export = $"{Base}/export";
            public const string ExportRemains = $"{Base}/exportremains";
        }

        public static class Products
        {
            private const string Base = $"{Ver}/products";

            public const string GetAll = Base;
            public const string GetById = $"{Base}/{{Id}}";
            public const string Create = Base;
            public const string Update = $"{Base}/{{Id}}";
            public const string Delete = $"{Base}/{{Id}}";
            public const string ExportRemains = $"{Base}/exportremains";
        }

        public static class Operations
        {
            private const string Base = $"{Ver}/operations";

            public const string GetAll = Base;
            public const string GetAll1 = $"{Base}1";
            public const string GetAllByType = $"{{OperType}}/{Base}";
            public const string GetById = $"{Base}/{{Id}}";
            public const string Create = Base;
            public const string Update = $"{Base}/{{Id}}";
            public const string Delete = $"{Base}/{{Id}}";
            public const string Export = $"{Base}/export";
        }

        public static class SendOrReceive
        {
            private const string Base = $"{Ver}/sendorreceive";

            public const string GetAll = Base;
            public const string GetAllByType = $"{{OperType}}/{Base}";
            public const string GetById = $"{Base}/{{Id}}";
            public const string Create = Base;
            public const string Update = $"{Base}/{{Id}}";
            public const string Delete = $"{Base}/{{Id}}";
        }

        public static class Orders
        {
            private const string Base = $"{Ver}/orders";

            public const string GetAll = Base;
            public const string GetAllByType = $"{{OrderType}}/{Base}";
            public const string GetById = $"{Base}/{{Id}}";
            public const string Create = Base;
            public const string Update = $"{Base}/{{Id}}";
            public const string Delete = $"{Base}/{{Id}}";
            public const string Export = $"{Base}/export";
        }

        public static class Reports
        {
            private const string Base = $"{Ver}/reports";

            public const string Get = Base;
            public const string Remains = $"{Base}/remains";
            public const string Debts = $"{Base}/debts";
            public const string Turnovers = $"{Base}/turnovers";
            public const string Dashboard = $"{Base}/dashboard";
            public const string AgentTurnovers = $"{Base}/agentturnovers";
            public const string History = $"{Base}/history";
            public const string Export = $"{Base}/{{templateName}}";
            public const string Print = $"{Base}/print/{{templateName}}/{{operationId}}";
            public const string PrintHtml = $"{Base}/print/html/{{templateName}}";
        }

        public static class Audits
        {
            private const string BaseAgent = $"{Ver}/audit/agents";
            private const string BaseGoods = $"{Ver}/audit/goods";

            public const string GetAllAgents = BaseAgent;
            public const string GetAllGoods = BaseGoods;
            public const string GetAgentsById = $"{BaseAgent}/{{Id}}";
            public const string GetGoodsById = $"{BaseGoods}/{{Id}}";
            public const string CreateAgents = BaseAgent;
            public const string CreateGoods = BaseGoods;
            public const string UpdateAgents = $"{BaseAgent}/{{Id}}";
            public const string UpdateGoods = $"{BaseGoods}/{{Id}}";
            public const string DeleteAgents = $"{BaseAgent}/{{Id}}";
            public const string DeleteGoods = $"{BaseGoods}/{{Id}}";
        }
    }
}
