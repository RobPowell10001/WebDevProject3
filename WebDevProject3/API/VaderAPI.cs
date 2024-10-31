using VaderSharp2;

namespace API;

public class VaderAPI
{
    //Returns a triple double containing the positive, negative, and compound sentiment values
    public Tuple<double, double, double> AnalyzeSentiment(string inputText)
    {
        // Initialize the sentiment analyzer
        var analyzer = new SentimentIntensityAnalyzer();

        // Analyze the sentiment of the input text
        var results = analyzer.PolarityScores(inputText);

        // Prepare the output
        var sentimentResult = new Tuple<double, double, double>(results.Positive, results.Negative, results.Compound);

        // Pass the result to the view or return as JSON
        return sentimentResult;
    }
}
