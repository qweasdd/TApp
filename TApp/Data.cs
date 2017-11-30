namespace TApp
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Data
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int FileLength { get; set; }

        public int CommentaryLines { get; set; }

        public double CommentaryProportion { get; set; }
        

        public double AverageClassLength { get; set; }

        public double AverageMethodLength { get; set; }

        public double MaintainabilityIndex { get; set; }

        public double CyclomaticComplexity { get; set; }

        public int ClassAmount { get; set; }

        public int InterfaceAmount { get; set; }

        public int StructAmount { get; set; }

        public int NamespaceAmount { get; set; }

        public double AverageClassNameLength { get; set; }

        public double AverageVariableNameLength { get; set; }

        public double AverageMethodNameLength { get; set; }

        public double AverageStructNameLength { get; set; }

        public double AverageStructLength { get; set; }

        public double AverageInterfaceLength { get; set; }

        public double AverageInterfaceNameLength { get; set; }

        public int LinesOfCode { get; set; }

        public string Names { get; set; }

        public int PublicAmount { get; set; }

        public int PrivateAmount { get; set; }

        public int InternalAmount { get; set; }

        public int ProtectedAmount { get; set; }

        public int AbstractAmount { get; set; }

        public int ConstAmount { get; set; }

        public int EventAmount { get; set; }

        public int ExternAmount { get; set; }

        public int NewAmount { get; set; }

        public int OverrideAmount { get; set; }

        public int PartialAmount { get; set; }

        public int ReadonlyAmount { get; set; }

        public int SealedAmount { get; set; }

        public int StaticAmount { get; set; }

        public int UnsafeAmount { get; set; }

        public int VirtualAmount { get; set; }

        public int VolatileAmount { get; set; }

        public double AverageMethodParametersAmount { get; set; }

        public int AsyncAmount { get; set; }

        public double AverageClassFieldsAmount { get; set; }

        public double AverageStructFieldsAmount { get; set; }

        public double AverageClassPropertiesAmount { get; set; }

        public double AverageStructPropertiesAmount { get; set; }

        public double AverageInterfacePropertiesAmount { get; set; }

        public double AverageClassMethodsAmount { get; set; }

        public double AverageStructMethodsAmount { get; set; }

        public double AverageInterfaceMethodsAmount { get; set; }

        public int MedianClassLength { get; set; }

        public int MedianMethodLength { get; set; }

        public int MedianClassNameLength { get; set; }

        public int MedianVariableNameLength { get; set; }

        public int MedianStructNameLength { get; set; }

        public int MedianStructLength { get; set; }

        public int MedianInterfaceLength { get; set; }

        public int MedianInterfaceNameLength { get; set; }

        public int MedianMethodParametersAmount { get; set; }

        public int MedianClassFieldsAmount { get; set; }

        public int MedianStructFieldsAmount { get; set; }

        public int MedianClassPropertiesAmount { get; set; }

        public int MedianStructPropertiesAmount { get; set; }

        public int MedianInterfacePropertiesAmount { get; set; }

        public int MedianClassMethodsAmount { get; set; }

        public int MedianStructMethodsAmount { get; set; }

        public int MedianInterfaceMethodsAmount { get; set; }

        public double AverageLineLength { get; set; }

        public int MedianLineLength { get; set; }

        public int MedianMethodNameLength { get; set; }

        public int PrefixUnderscoreNamesAmount { get; set; }

        public double PrefixUnderscoreNamesFraction { get; set; }

        public int UnderscoreNamesAmount { get; set; }

        public double UnderscoreNamesFraction { get; set; }

        public int LowercaseNamesAmount { get; set; }

        public double LowercaseNamesFraction { get; set; }

        public int UppercaseWithUnderscoreNamesAmount { get; set; }

        public double UppercaseWithUnderscoreNamesFraction { get; set; }

        public int PascalCaseNamesAmount { get; set; }

        public double PascalCaseNamesFraction { get; set; }

        public int CamelCaseNamesAmount { get; set; }

        public double CamelCaseNamesFraction { get; set; }

        public string WordsFromNames { get; set; }

        public double AverageCommentaryLength { get; set; }

        public int MedianCommentaryLength { get; set; }

        public string CommentaryContent { get; set; }

        public virtual Download Download { get; set; }
    }
}
