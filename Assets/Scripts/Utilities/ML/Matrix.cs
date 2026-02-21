using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Utilities
{
    namespace ML
    {
#region Matrix
        public readonly struct Matrix
        {
            public readonly double[] Data {get {return _data;}}

            private readonly double[] _data;

            public int Rows { get; }
            public int Columns { get; }
            public (int, int) Shape { get { return (Rows, Columns); } }

            public static Matrix Identity(int size)
            {
                Matrix m = new Matrix(size, size);
                for (int i = 0; i < size; i++)
                    m._data[i * size + i] = 1.0;
                return m;
            }
            public static Matrix T(Matrix matrix)
            {
                Matrix res = new Matrix(matrix.Columns, matrix.Rows);

                for (int i = 0; i < matrix.Rows; i++)
                {
                    for (int j = 0; j < matrix.Columns; j++)
                    {
                        res[j,i] = matrix[i,j];
                    }
                }

                return res;
            }

#region Constructors
            public Matrix(int rows, int columns)
            {
                if (rows <= 0 || columns <= 0)
                    throw new ArgumentOutOfRangeException();

                Rows = rows;
                Columns = columns;
                _data = new double[rows * columns];
            }

            public Matrix((int, int) size)
            {
                if (size.Item1 <= 0 || size.Item2 <= 0)
                    throw new ArgumentOutOfRangeException();

                Rows = size.Item1;
                Columns = size.Item2;
                _data = new double[size.Item1 * size.Item2];
            }

            public Matrix(double[,] data)
            {
                Rows = data.GetLength(0);
                Columns = data.GetLength(1);
                _data = new double[Rows * Columns];

                for (int i = 0; i < Rows; i++)
                    for (int j = 0; j < Columns; j++)
                        _data[i*Columns + j] = data[i, j];
            }

            public Matrix(double[] data)
            {
                Rows = data.Length;
                Columns = 1;
                _data = (double[])data.Clone();
            }
#endregion

            public double this[int row, int column]
            {
                get => _data[row * Columns + column];
                set => _data[row * Columns + column] = value;
            }
            public double this[int row]
            {
                get => _data[row];
                set => _data[row] = value;
            }

#region Operators
            // Dot product of two matrices
            public static Matrix operator *(Matrix left, Matrix right)
            {
                if (left.Columns != right.Rows)
                    throw new InvalidOperationException("Invalid matrix dimensions for multiplication!");

                int rowsLeft = left.Rows;
                int colsLeft = left.Columns;
                int colsRight = right.Columns;

                Matrix result = new Matrix(rowsLeft, colsRight);

                double [] dataLeft = left._data;
                double [] dataRight = right._data;
                double [] dataResult = result._data;

                for (int i = 0; i < rowsLeft; i++)
                {
                    int leftRow = i * colsLeft;
                    int resultRow = i * colsRight;

                    for (int k = 0; k < colsLeft; k++)
                    {
                        double leftK = dataLeft[leftRow + k];
                        int rightRow = k * colsRight;

                        for (int j = 0; j < colsRight; j++)
                            dataResult[resultRow + j] += leftK * dataRight[rightRow + j];
                    }
                }

                return result;
            }
            // Parallel dot product of two matrices
            public static Matrix ParallelDot(Matrix left, Matrix right)
            {
                if (left.Columns != right.Rows)
                    throw new InvalidOperationException("Invalid matrix dimensions for multiplication!");

                int rowsLeft = left.Rows;
                int colsLeft = left.Columns;
                int colsRight = right.Columns;

                Matrix result = new Matrix(rowsLeft, colsRight);

                double [] dataLeft = left._data;
                double [] dataRight = right._data;
                double [] dataResult = result._data;

                Parallel.For(0, rowsLeft, i =>
                {
                    int leftRow = i * colsLeft;
                    int resultRow = i * colsRight;

                    for (int k = 0; k < colsLeft; k++)
                    {
                        double leftK = dataLeft[leftRow + k];
                        int rightRow = k * colsRight;

                        for (int j = 0; j < colsRight; j++)
                            dataResult[resultRow + j] += leftK * dataRight[rightRow + j];
                    }
                });

                return result;
            }
            // Linear multiply of matrix
            public static Matrix LinMult(Matrix left, Matrix right)
            {
                if (left.Shape != right.Shape)
                    throw new InvalidOperationException("Invalid matrix dimensions for linear multiplication!");

                Matrix result = new Matrix(left.Rows, left.Columns);
                for (int i = 0; i < left.Rows; i++)
                    for (int j = 0; j < left.Columns; j ++)
                        result[i, j] = left[i,j] * right[i, j];
                return result;
            }
            // Scalar multiply of matrix
            public static Matrix operator *(double scalar, Matrix matrix)
            {
                Matrix result = new Matrix(matrix.Rows, matrix.Columns);
                for (int i = 0; i < matrix.Rows; i++)
                    for (int j = 0; j < matrix.Columns; j ++)
                        result[i, j] = scalar * matrix[i, j];
                return result;
            }
            public static Matrix operator *(Matrix matrix, double scalar) => scalar * matrix;

            // Scalar division of matrix
            public static Matrix operator /(Matrix matrix, double scalar)
            {
                return (1/scalar) * matrix;
            }

            // Addition of two matrices
            public static Matrix operator +(Matrix left, Matrix right)
            {
                int rows = left.Rows;
                int cols = left.Columns;
                if (left.Shape != right.Shape)
                    throw new InvalidOperationException("Invalid matrix dimensions for addition!");

                Matrix result = new Matrix(rows, cols);
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j ++)
                        result[i, j] = left[i, j] + right[i, j];
                return result;
            }
            // Scalar addition of matrix
            public static Matrix operator +(double scalar, Matrix matrix)
            {
                Matrix result = new Matrix(matrix.Rows, matrix.Columns);
                for (int i = 0; i < matrix.Rows; i++)
                    for (int j = 0; j < matrix.Columns; j ++)
                        result[i, j] = scalar + matrix[i, j];
                return result;
            }
            public static Matrix operator +(Matrix matrix, double scalar) => scalar + matrix;

            // Subtraction of two matrices
            public static Matrix operator -(Matrix left, Matrix right)
            {
                int rows = left.Rows;
                int cols = left.Columns;
                if (rows != right.Rows || cols != right.Columns)
                    throw new InvalidOperationException("Invalid matrix dimensions for subtraction!");

                Matrix result = new Matrix(rows, cols);
                for (int i = 0; i < rows; i++)
                    for (int j = 0; j < cols; j ++)
                        result[i, j] = left[i, j] - right[i, j];
                return result;
            }
            // Scalar subtraction of matrix
            public static Matrix operator -(Matrix matrix, double scalar)
            {
                Matrix result = new Matrix(matrix.Rows, matrix.Columns);
                for (int i = 0; i < matrix.Rows; i++)
                    for (int j = 0; j < matrix.Columns; j ++)
                        result[i, j] = matrix[i, j] - scalar;
                return result;
            }
#endregion

#region Helper Functions
            public void ForEach(Action<int, int> action)
            {
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        action(i, j);
                    }
                }
            }

            public void Fill(double value)
            {
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        this[i,j] = 1;
                    }
                }
            }
            
            public override string ToString()
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                        sb.Append(this[i, j].ToString("0.###")).Append('\t');
                    sb.AppendLine();
                }
                return sb.ToString();
            }
        }
#endregion
#endregion
    }
}
