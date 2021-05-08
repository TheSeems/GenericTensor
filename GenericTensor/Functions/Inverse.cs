﻿#region copyright
/*
 * MIT License
 * 
 * Copyright (c) 2020-2021 WhiteBlackGoose
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
#endregion


using System.Runtime.CompilerServices;
using GenericTensor.Core;

namespace GenericTensor.Functions
{
    internal static class Inversion<T, TWrapper> where TWrapper : struct, IOperations<T>
    {
        /// <summary>
        /// Borrowed from here: https://www.geeksforgeeks.org/adjoint-inverse-matrix/
        ///
        /// O(N^2)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetCofactorMatrix(GenTensor<T, TWrapper> a, GenTensor<T, TWrapper> temp, int rowId,
            int colId, int diagLength)
        {
            int i = 0, j = 0;
            for (int row = 0; row < diagLength; row++)
            {
                for (int col = 0; col < diagLength; col++)
                {
                    if (row != rowId && col != colId)
                    {
                        temp.SetValueNoCheck(a.GetValueNoCheck(row, col), i, j);
                        j++;
                        if (j == diagLength - 1)
                        {
                            j = 0;
                            i++;
                        }
                    }
                }
            }
        }

        
        public static GenTensor<T, TWrapper> Adjoint(GenTensor<T, TWrapper> t)
        {
            #if ALLOW_EXCEPTIONS
            if (!t.IsSquareMatrix)
                throw new InvalidShapeException("Matrix should be square");
            #endif
            var diagLength = t.Shape.shape[0];

            if (diagLength is 1)
                return GenTensor<T, TWrapper>.CreateIdentityMatrix(1);

            var res = GenTensor<T, TWrapper>.CreateSquareMatrix(diagLength);
            var temp = SquareMatrixFactory<T, TWrapper>.GetMatrix(diagLength - 1);

            if (diagLength == 1)
            {
                res.SetValueNoCheck(default(TWrapper).CreateOne(), 0, 0);
                return res;
            }

            var toNegate = false;

            for (int x = 0; x < diagLength; x++)
            for (int y = 0; y < diagLength; y++)
            {
                GetCofactorMatrix(t, temp, x, y, diagLength);                

                var cofactor = Determinant<T, TWrapper>.DeterminantGaussianSafeDivision(temp, diagLength - 1);
                // TODO: is this statement correct?
                toNegate = (x + y) % 2 == 1;
                var minor = toNegate ? default(TWrapper).Negate(cofactor) : cofactor;

                res.SetValueNoCheck(minor, y, x);
            }

            return res;
        }

        public static void InvertMatrix(GenTensor<T, TWrapper> t)
        {
            #if ALLOW_EXCEPTIONS
            if (!t.IsSquareMatrix)
                throw new InvalidShapeException("this should be a square matrix");
            #endif

            var diagLength = t.Shape.shape[0];

            if (diagLength is 1)
            {
                t.SetValueNoCheck(
                    default(TWrapper).Divide(
                    default(TWrapper).CreateOne(),
                    t.GetValueNoCheck(0, 0)
                    ), 0, 0);
                return;
            }

            var det = Determinant<T, TWrapper>.DeterminantGaussianSafeDivision(t);
            #if ALLOW_EXCEPTIONS
            if (default(TWrapper).IsZero(det))
                throw new InvalidDeterminantException("Cannot invert a singular matrix");
            #endif

            var adj = Adjoint(t);
            for (int x = 0; x < diagLength; x++)
            for (int y = 0; y < diagLength; y++)
                t.SetValueNoCheck(
                    default(TWrapper).Divide(
                        adj.GetValueNoCheck(x, y),
                        det
                    ),
                    x, y
                );
        }

        public static GenTensor<T, TWrapper> MatrixDivide(GenTensor<T, TWrapper> a, GenTensor<T, TWrapper> b)
        {
            #if ALLOW_EXCEPTIONS
            if (!a.IsSquareMatrix || !b.IsSquareMatrix)
                throw new InvalidShapeException("Both should be square matrices");
            if (a.Shape != b.Shape)
                throw new InvalidShapeException("Given matrices should be of the same shape");
            #endif
            var fwd = b.Forward();
            fwd.InvertMatrix();
            return MatrixMultiplication<T, TWrapper>.Multiply(a, fwd);
        }

        public static GenTensor<T, TWrapper> TensorMatrixDivide(GenTensor<T, TWrapper> a, GenTensor<T, TWrapper> b)
        {
            #if ALLOW_EXCEPTIONS
            InvalidShapeException.NeedTensorSquareMatrix(a);
            InvalidShapeException.NeedTensorSquareMatrix(b);
            if (a.Shape != b.Shape)
                throw new InvalidShapeException("Should be of the same shape");
            #endif

            var res = new GenTensor<T, TWrapper>(a.Shape);
            foreach (var ind in res.IterateOverMatrices())
                res.SetSubtensor(
                    MatrixDivide(
                        a.GetSubtensor(ind),
                        b.GetSubtensor(ind)
                        ), ind);

            return res;
        }
        
        public static void TensorMatrixInvert(GenTensor<T, TWrapper> t)
        {
            #if ALLOW_EXCEPTIONS
            InvalidShapeException.NeedTensorSquareMatrix(t);
            #endif

            foreach (var ind in t.IterateOverMatrices())
                t.GetSubtensor(ind).InvertMatrix();
        }
    }
}
