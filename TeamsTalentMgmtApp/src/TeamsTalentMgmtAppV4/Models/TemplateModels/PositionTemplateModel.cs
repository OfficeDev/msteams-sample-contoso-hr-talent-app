using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV4.Models.TemplateModels
{
    public class PositionTemplateModel : BaseTemplateModel<Position>
    {
        public object ButtonActions { get; set; }
    }
}
