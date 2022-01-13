using System.Collections.ObjectModel;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV4.Models.TemplateModels
{
    public class CandidateTemplateModel : BaseTemplateModel<Candidate>
    {
        public ReadOnlyCollection<Recruiter> Interviewers { get; set; }

        public AppSettings AppSettings { get; set; }

        public string Locale { get; set; }
    }
}
