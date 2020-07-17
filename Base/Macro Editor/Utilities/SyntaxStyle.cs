using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macro_Editor.Utilities
{
    public class SyntaxStyleValues : Model.Base.Model
    {
        private SyntaxStyleValuesBase Model;
        public SyntaxStyleValues()
        {
            Model = new SyntaxStyleValuesBase();

            Model.DIGIT = "#000000";
            Model.COMMENT = "#000000";
            Model.STRING = "#000000";
            Model.PAIR = "#000000";
            Model.CLASS = "#000000";
            Model.STATEMENT = "#000000";
            Model.FUNCTION = "#000000";
            Model.BOOLEAN = "#000000";
        }

        public string DIGIT { get { return Model.DIGIT; }
            set
            {
                if (Model.DIGIT != value)
                {
                    Model.DIGIT = value;
                    OnPropertyChanged(nameof(DIGIT));

                    SyntaxStyleLoader.SetSyntaxStyle(this);
                }
            }
        }

        public string COMMENT
        {
            get { return Model.COMMENT; }
            set
            {
                if (Model.COMMENT != value)
                {
                    Model.COMMENT = value;
                    OnPropertyChanged(nameof(COMMENT));

                    SyntaxStyleLoader.SetSyntaxStyle(this);
                }
            }
        }

        public string STRING
        {
            get { return Model.STRING; }
            set
            {
                if (Model.STRING != value)
                {
                    Model.STRING = value;
                    OnPropertyChanged(nameof(STRING));

                    SyntaxStyleLoader.SetSyntaxStyle(this);
                }
            }
        }

        public string PAIR
        {
            get { return Model.PAIR; }
            set
            {
                if (Model.PAIR != value)
                {
                    Model.PAIR = value;
                    OnPropertyChanged(nameof(PAIR));

                    SyntaxStyleLoader.SetSyntaxStyle(this);
                }
            }
        }

        public string CLASS
        {
            get { return Model.CLASS; }
            set
            {
                if (Model.CLASS != value)
                {
                    Model.CLASS = value;
                    OnPropertyChanged(nameof(CLASS));

                    SyntaxStyleLoader.SetSyntaxStyle(this);
                }
            }
        }

        public string STATEMENT
        {
            get { return Model.STATEMENT; }
            set
            {
                if (Model.STATEMENT != value)
                {
                    Model.STATEMENT = value;
                    OnPropertyChanged(nameof(STATEMENT));

                    SyntaxStyleLoader.SetSyntaxStyle(this);
                }
            }
        }

        public string FUNCTION
        {
            get { return Model.FUNCTION; }
            set
            {
                if (Model.FUNCTION != value)
                {
                    Model.FUNCTION = value;
                    OnPropertyChanged(nameof(FUNCTION));

                    SyntaxStyleLoader.SetSyntaxStyle(this);
                }
            }
        }

        public string BOOLEAN
        {
            get { return Model.BOOLEAN; }
            set
            {
                if (Model.BOOLEAN != value)
                {
                    Model.BOOLEAN = value;
                    OnPropertyChanged(nameof(BOOLEAN));

                    SyntaxStyleLoader.SetSyntaxStyle(this);
                }
            }
        }
    }

    struct SyntaxStyleValuesBase
    {
        public string DIGIT;
        public string COMMENT;
        public string STRING;
        public string PAIR;
        public string CLASS;
        public string STATEMENT;
        public string FUNCTION;
        public string BOOLEAN;
    }
}
