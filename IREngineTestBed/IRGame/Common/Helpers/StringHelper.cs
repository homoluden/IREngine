using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IRGame.Common.Helpers
{
    public static class StringHelper
    {
        #region Consts
        public static readonly string MATLAB_MATRIX_PATTERN1 = @"\[\s*([e\s\.\d,]*)\s*\]";
        public static readonly string MATLAB_MATRIX_PATTERN2 = @"^([\d\.\s]+),*$";
        public static readonly string MATLAB_MATRIX_PATTERN3 = @"^\s*([\d\.]*)\s*$";
        #endregion

        public static double[,] ParseMatlabMatrix(string matrixStr)
        {
            var matContentRegex = new Regex(MATLAB_MATRIX_PATTERN1);
            var matRowsRegex = new Regex(MATLAB_MATRIX_PATTERN2);
            var matCellsRegex = new Regex(MATLAB_MATRIX_PATTERN3);
            // TODO: Parse Matlab Denoted Matrix
            var result = new double[0,0];

            return result;
        }
    }
}
