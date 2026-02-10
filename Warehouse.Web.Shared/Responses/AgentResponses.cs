namespace Warehouse.Web.Shared.Responses;

public class OperAgentResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public long StoreId { get; set; }
    public string StoreName { get; set; }
    public long ManagerId { get; set; }
    public string ManagerName { get; set; }
    public string ManagerPhone { get; set; }
    public string Comment { get; set; }

    //For audit
    public decimal Plan { get; set; }
    public decimal Fact { get; set; }
    public decimal Difference { get; set; }
    public bool Changed { get; set; }
}

public class AgentDebtsResponse
{
    public long Id { get; set; }
    public decimal DebtOnBegin { get; set; }
    public decimal Debt { get; set; }
    public decimal Credit { get; set; }
    public decimal DebtOnEnd { get; set; }
    public short Level { get; set; }
}

public class AgentResponse
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public long StoreId { get; set; }
    public string StoreName { get; set; }
    public long ManagerId { get; set; }
    public string ManagerName { get; set; }
    public string ManagerPhone { get; set; }
    public string Comment { get; set; }

    public AgentDebtsResponse Debts { get; set; } = new AgentDebtsResponse();

}
public class AgentsResponse
{
    public int Total { get; set; }
    public decimal TotalDebt { get; set; }
    public decimal TotalCredit { get; set; }
    public List<StoreResponse> Stores { get; set; } = new List<StoreResponse>();
    public List<AgentResponse> Agents { get; set; } = new List<AgentResponse>();
    public List<AgentResponse> Items { get; set; } = new List<AgentResponse>();
}


//++++++++++++++++++++++++++++++

public class AgentsDebtsResponseOld
{
    public IEnumerable<AgentDebtsResponseOld> Items { get; set; } = Enumerable.Empty<AgentDebtsResponseOld>();
//    public IEnumerable<AgentCompactResponseOld> Agents { get; set; } = Enumerable.Empty<AgentCompactResponseOld>();
//    public IEnumerable<ManagerCompactResponseOld> Managers { get; set; } = Enumerable.Empty<ManagerCompactResponseOld>();
//    public IEnumerable<StoreResponseOld> Stores { get; set; } = Enumerable.Empty<StoreResponseOld>();
}

public class AgentDebtsResponseOld
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string? Address { get; set; }
    public string? Comment { get; set; }
    public long ManagerId { get; set; }
    public long StoreId { get; set; }
    public string Manager { get; set; }
    public string Store { get; set; }
    public decimal DebtOnBegin { get; set; }
    public decimal Debt { get; set; }
    public decimal Credit { get; set; }
    public decimal DebtOnEnd { get; set; }
}