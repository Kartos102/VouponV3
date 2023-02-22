using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class SurveyQuestions
    {
        public SurveyQuestions()
        {
            SurveyLogicFlows = new HashSet<SurveyLogicFlows>();
            SurveyQuestionAnswers = new HashSet<SurveyQuestionAnswers>();
            SurveyQuestionMultiLanguagesAnswers = new HashSet<SurveyQuestionMultiLanguagesAnswers>();
            SurveyQuestionMultiLanguagesTitles = new HashSet<SurveyQuestionMultiLanguagesTitles>();
            SurveyResponseAnswers = new HashSet<SurveyResponseAnswers>();
        }

        public int Id { get; set; }
        public int? SurveyQuestionTierId { get; set; }
        public int CommercialId { get; set; }
        public short QuestionTypeId { get; set; }
        public decimal QuestionTypePrice { get; set; }
        public short QuestionDelayId { get; set; }
        public decimal Point { get; set; }
        public bool IsChained { get; set; }
        public decimal? ChainedPrice { get; set; }
        public bool IsSequence { get; set; }
        public string TemplateContent { get; set; } = null!;
        public string QuestionTemplateContent { get; set; } = null!;
        public string? QuestionTemplateContentMobile { get; set; }
        public string? QuestionTitle { get; set; }
        public short? DemographicTypeId { get; set; }
        public int? PipeFrom { get; set; }
        public int? ChunkId { get; set; }
        public string? PipeCode { get; set; }

        public virtual SurveyChunkings? Chunk { get; set; }
        public virtual Commercials Commercial { get; set; } = null!;
        public virtual DemographicTypes? DemographicType { get; set; }
        public virtual QuestionDelays QuestionDelay { get; set; } = null!;
        public virtual QuestionTypes QuestionType { get; set; } = null!;
        public virtual SurveyQuestionTiers? SurveyQuestionTier { get; set; }
        public virtual ICollection<SurveyLogicFlows> SurveyLogicFlows { get; set; }
        public virtual ICollection<SurveyQuestionAnswers> SurveyQuestionAnswers { get; set; }
        public virtual ICollection<SurveyQuestionMultiLanguagesAnswers> SurveyQuestionMultiLanguagesAnswers { get; set; }
        public virtual ICollection<SurveyQuestionMultiLanguagesTitles> SurveyQuestionMultiLanguagesTitles { get; set; }
        public virtual ICollection<SurveyResponseAnswers> SurveyResponseAnswers { get; set; }
    }
}
