using System.ComponentModel.DataAnnotations.Schema;
using EK.Discord.Server.Notion.Base.Persistence;
using Notion.Client;

namespace EK.Discord.Server.Notion;

public class TestNotionRepo : AbstractNotionRepository<TestTo> {

    public TestNotionRepo(INotionClient notionClient) : base(notionClient) {
    }

    public IReadOnlyList<TestTo> GetAll() {
        return RunQuery(new DatabasesQueryParameters()).ToList();
    }

    public void Create(TestTo newValue) {
        base.Create(newValue);
    }
}

// TODO use schema to select the notion client
[Table("0b5a5f4136af417b90ed38383fe69312")]
public class TestTo : IEntity {
    
    [Column("Skill", TypeName = nameof(PropertyValueType.Title))]
    public string Skill { get; set; } = "";

    
    
    
    public override string ToString() {
        return Skill;
    }

}