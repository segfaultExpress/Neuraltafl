using System;
namespace NeuralTaflGame.Domain.Common;


public abstract class Common
{
    /// <summary>
    /// Converts a dictionary vector string ("1,1") into a 
    /// string to display to the user (B2)
    /// </summary>
    /// <param name="vector">The vector of a square</param>
    /// <returns>A displayable string that displays a piece location</returns>
    public static String reverseConvertVector(String vector)
    {
        // Quick dirty way to display the potential moves
        String[] vectorValues = vector.Split(",");
        
        int col;
        int.TryParse(vectorValues[1], out col);
        char colAscii = (char) (col + 65);

        int row;
        int.TryParse(vectorValues[0], out row);
        row++;

        return colAscii + "" + row;

    }

    /// <summary>
    /// Converts a display turn (B2) into a vector (1,1)
    /// </summary>
    /// <param name="squareID">The string of the location of the square</param>
    /// <returns>A displayable string that contains</returns>
    public static int[] convertVector(String squareID)
    {
        // I'm not going to do extensive validation on these, since hopefully we graduate from text input pretty quickly in the dev cycle
        if (squareID.Count() == 0)
            return null;

        // This is actually a really funny college-level assignment
        int col = (int) Convert.ToChar(squareID[0]) - 65;
        col = (col > 26) ? col - 32 : col; // lowercase inputs

        int row;
        bool success = int.TryParse(squareID.Substring(1), out row);
        if (!success)
            return null;
        row--;

        int[] turnArray = new int[2];
        turnArray[0] = row;
        turnArray[1] = col;

        return turnArray;
    }
}