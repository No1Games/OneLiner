public class ScoreCalculator
{
    private int _star1Lines = 15;
    private int _star2Lines = 10;

    public int CalculateStars(int linesCount)
    {
        if (linesCount > _star1Lines)
            return 1;
        else if (linesCount > _star2Lines)
            return 2;
        else
            return 3;
    }
}
