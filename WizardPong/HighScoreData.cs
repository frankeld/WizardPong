using System.Collections.Generic;

namespace WizardPong
{

    public class HighScoreData //Used for serialization
    {
        public List<string> PlayerNames;
        public List<int> Scores;

        public HighScoreData()
        {
            PlayerNames = new List<string>();
            Scores = new List<int>();
        }
    }
}
