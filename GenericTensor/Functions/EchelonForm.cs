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


using GenericTensor.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericTensor.Functions
{
    internal static class EchelonFormExtensions
    {
        internal static GenTensor<T, TWrapper> SafeDivisionToSimple<T, TWrapper>(
            this GenTensor<EchelonForm<T, TWrapper>.SafeDivisionWrapper<T, TWrapper>,
                EchelonForm<T, TWrapper>.WrapperSafeDivisionWrapper<T, TWrapper>> t)
            where TWrapper : struct, IOperations<T>
            => GenTensor<T, TWrapper>.CreateMatrix(t.Shape[0], t.Shape[1], (x, y) => t.GetValueNoCheck(x, y).Count());

        internal static
            GenTensor<EchelonForm<T, TWrapper>.SafeDivisionWrapper<T, TWrapper>,
                EchelonForm<T, TWrapper>.WrapperSafeDivisionWrapper<T, TWrapper>>
            SimpleToSafeDivision<T, TWrapper>(this GenTensor<T, TWrapper> t) where TWrapper : struct, IOperations<T>
            => GenTensor<EchelonForm<T, TWrapper>.SafeDivisionWrapper<T, TWrapper>,
                    EchelonForm<T, TWrapper>.WrapperSafeDivisionWrapper<T, TWrapper>>
                .CreateMatrix(t.Shape[0], t.Shape[1],
                    (x, y) => new EchelonForm<T, TWrapper>.SafeDivisionWrapper<T, TWrapper>(t.GetValueNoCheck(x, y))
                );
    }

    internal static class EchelonForm<T, TWrapper> where TWrapper : struct, IOperations<T>
    {
        #region Gaussian elimination safe division

        internal struct SafeDivisionWrapper<W, TW> where TW : struct, IOperations<W>
        {
            internal W num;
            internal W den;

            public SafeDivisionWrapper(W val)
            {
                num = val;
                den = default(TW).CreateOne();
            }

            public SafeDivisionWrapper(W num, W den)
            {
                this.num = num;
                this.den = den;
            }

            public W Count() => default(TW).Divide(num, den);
        }

        internal struct WrapperSafeDivisionWrapper<W, TW> : IOperations<SafeDivisionWrapper<W, TW>>
            where TW : struct, IOperations<W>
        {
            public SafeDivisionWrapper<W, TW> Add(SafeDivisionWrapper<W, TW> a, SafeDivisionWrapper<W, TW> b)
            {
                return new SafeDivisionWrapper<W, TW>(
                    default(TW).Add(default(TW).Multiply(a.num, b.den), default(TW).Multiply(b.num, a.den)),
                    default(TW).Multiply(a.den, b.den));
            }

            public SafeDivisionWrapper<W, TW> Subtract(SafeDivisionWrapper<W, TW> a, SafeDivisionWrapper<W, TW> b)
            {
                return new SafeDivisionWrapper<W, TW>(
                    default(TW).Subtract(default(TW).Multiply(a.num, b.den), default(TW).Multiply(b.num, a.den)),
                    default(TW).Multiply(a.den, b.den));
            }

            public SafeDivisionWrapper<W, TW> Multiply(SafeDivisionWrapper<W, TW> a, SafeDivisionWrapper<W, TW> b)
            {
                return new SafeDivisionWrapper<W, TW>(default(TW).Multiply(a.num, b.num),
                    default(TW).Multiply(a.den, b.den));
            }

            public SafeDivisionWrapper<W, TW> Negate(SafeDivisionWrapper<W, TW> a)
            {
                return new SafeDivisionWrapper<W, TW>(default(TW).Negate(a.num), a.den);
            }

            public SafeDivisionWrapper<W, TW> Divide(SafeDivisionWrapper<W, TW> a, SafeDivisionWrapper<W, TW> b)
            {
                return new SafeDivisionWrapper<W, TW>(default(TW).Multiply(a.num, b.den),
                    default(TW).Multiply(a.den, b.num));
            }

            public SafeDivisionWrapper<W, TW> CreateOne()
            {
                return new SafeDivisionWrapper<W, TW>(default(TW).CreateOne());
            }

            public SafeDivisionWrapper<W, TW> CreateZero()
            {
                return new SafeDivisionWrapper<W, TW>(default(TW).CreateZero());
            }

            public SafeDivisionWrapper<W, TW> Copy(SafeDivisionWrapper<W, TW> a)
            {
                return new SafeDivisionWrapper<W, TW>(default(TW).Copy(a.num), default(TW).Copy(a.den));
            }

            public SafeDivisionWrapper<W, TW> Forward(SafeDivisionWrapper<W, TW> a)
            {
                return a;
            }

            public bool AreEqual(SafeDivisionWrapper<W, TW> a, SafeDivisionWrapper<W, TW> b)
            {
                return default(TW).AreEqual(a.num, b.num) && default(TW).AreEqual(a.den, b.den);
            }

            public bool IsZero(SafeDivisionWrapper<W, TW> a)
            {
                return default(TW).IsZero(a.num);
            }

            public string ToString(SafeDivisionWrapper<W, TW> a)
            {
                return default(TW).ToString(a.num) + " / " + default(TW).ToString(a.den);
            }

            public byte[] Serialize(SafeDivisionWrapper<W, TW> a)
            {
                throw new System.NotImplementedException();
            }

            public SafeDivisionWrapper<W, TW> Deserialize(byte[] data)
            {
                throw new System.NotImplementedException();
            }
        }

        internal static GenTensor<SafeDivisionWrapper<T, TWrapper>, WrapperSafeDivisionWrapper<T, TWrapper>>
            InnerGaussianEliminationSafeDivision(GenTensor<T, TWrapper> t, int m, int n, out int swapCount)
        {
            var elemMatrix = t.SimpleToSafeDivision();
            swapCount = 0;
            EchelonForm<SafeDivisionWrapper<T, TWrapper>, WrapperSafeDivisionWrapper<T, TWrapper>>
                .InnerGaussianEliminationSimple(elemMatrix, 0, ref swapCount);
            return elemMatrix;
        }

        public static GenTensor<T, TWrapper> RowEchelonFormSafeDivision(GenTensor<T, TWrapper> t)
        {
#if ALLOW_EXCEPTIONS
            if (!t.IsMatrix)
                throw new InvalidShapeException("this should be matrix");
#endif
            var wrp = InnerGaussianEliminationSafeDivision(t, t.Shape[0], t.Shape[1], out _);
            return wrp.SafeDivisionToSimple();
        }

        internal static void InnerGaussianEliminationSimpleDiscardSwapCount(GenTensor<T, TWrapper> t, int off)
        {
            var intoNowhere = 0;
            InnerGaussianEliminationSimple(t, off, ref intoNowhere);
        }

        internal static void InnerGaussianEliminationSimple(GenTensor<T, TWrapper> t, int off, ref int swapCount)
        {
            // Here we are sticking to the algorithm,
            // provided here: https://www.math.purdue.edu/~shao92/documents/Algorithm%20REF.pdf
            // We can afford it, since it is implemented with tail-recursion.


            // II. No non-zero columns => the matrix is zero
            if (LeftmostNonZeroColumn(t, off) is not var (columnId, pivotId))
                return;


            // III. If the first non-zero element in a column is not in the first row,
            // we swap those rows to make it in the first row
            if (pivotId != off)
            {
                t.RowSwap(off, pivotId);
                swapCount++;
            }


            // IV. Now we shall go over all rows below off to make their
            // first element equal 0
            var pivotValue = t.GetValueNoCheck(off, columnId);
            for (int r = off + 1; r < t.Shape[0]; r++)
                if (!default(TWrapper).IsZero(t.GetValueNoCheck(r, columnId)))
                {
                    var currElement = t.GetValueNoCheck(r, columnId);
                    t.RowSubtract(r, off, default(TWrapper).Divide(currElement, pivotValue));
                }


            // VI. Let us apply the algorithm for the inner matrix
            InnerGaussianEliminationSimple(t, off + 1, ref swapCount);


            static int? NonZeroColumn(GenTensor<T, TWrapper> t, int c, int off)
            {
                for (int i = off; i < t.Shape[0]; i++)
                    if (!default(TWrapper).IsZero(t.GetValueNoCheck(i, c)))
                        return i;
                return null;
            }


            static (int columnId, int pivotId)? LeftmostNonZeroColumn(GenTensor<T, TWrapper> t, int off)
            {
                for (int c = off; c < t.Shape[1]; c++)
                    if (NonZeroColumn(t, c, off) is { } nonZero)
                        return (c, nonZero);
                return null;
            }
        }

        internal static void InnerGaussianEliminationSimplePermutation(GenTensor<T, TWrapper> t, int off,
            ref int[] permute)
        {
            // Here we are sticking to the algorithm,
            // provided here: https://www.math.purdue.edu/~shao92/documents/Algorithm%20REF.pdf
            // We can afford it, since it is implemented with tail-recursion.


            // II. No non-zero columns => the matrix is zero
            if (LeftmostNonZeroColumn(t, off) is not var (columnId, pivotId))
                return;


            // III. If the first non-zero element in a column is not in the first row,
            // we swap those rows to make it in the first row
            if (pivotId != off)
            {
                t.RowSwap(off, pivotId);

                // Swapping 2 elements standing for rows in our permutation array
                (permute[pivotId], permute[off]) = (permute[off], permute[pivotId]);
            }


            // IV. Now we shall go over all rows below off to make their
            // first element equal 0
            var pivotValue = t.GetValueNoCheck(off, columnId);
            for (int r = off + 1; r < t.Shape[0]; r++)
                if (!default(TWrapper).IsZero(t.GetValueNoCheck(r, columnId)))
                {
                    var currElement = t.GetValueNoCheck(r, columnId);
                    t.RowSubtract(r, off, default(TWrapper).Divide(currElement, pivotValue));
                }


            // VI. Let us apply the algorithm for the inner matrix
            InnerGaussianEliminationSimplePermutation(t, off + 1, ref permute);


            static int? NonZeroColumn(GenTensor<T, TWrapper> t, int c, int off)
            {
                for (int i = off; i < t.Shape[0]; i++)
                    if (!default(TWrapper).IsZero(t.GetValueNoCheck(i, c)))
                        return i;
                return null;
            }


            static (int columnId, int pivotId)? LeftmostNonZeroColumn(GenTensor<T, TWrapper> t, int off)
            {
                for (int c = off; c < t.Shape[1]; c++)
                    if (NonZeroColumn(t, c, off) is { } nonZero)
                        return (c, nonZero);
                return null;
            }
        }

        public static GenTensor<T, TWrapper> RowEchelonFormSimple(GenTensor<T, TWrapper> t)
        {
#if ALLOW_EXCEPTIONS
            if (!t.IsMatrix)
                throw new InvalidShapeException("this should be matrix");
#endif
            var res = t.Copy(copyElements: false);
            InnerGaussianEliminationSimpleDiscardSwapCount(res, 0);
            return res;
        }

        public static (GenTensor<T, TWrapper>, int[]) RowEchelonFormPermute(GenTensor<T, TWrapper> t)
        {
#if ALLOW_EXCEPTIONS
            if (!t.IsMatrix)
                throw new InvalidShapeException("this should be matrix");
#endif
            var res = t.Copy(copyElements: false);
            var permute = new int[t.Shape[0]];
            for (var i = 0; i < permute.Length; i++) permute[i] = i + 1;

            InnerGaussianEliminationSimplePermutation(res, 0, ref permute);
            return (res, permute);
        }

        public static (GenTensor<T, TWrapper>, int[]) RowEchelonFormPermuteSafeDivision(GenTensor<T, TWrapper> t)
        {
            var res = t.Copy(copyElements: false).SimpleToSafeDivision();
            var permute = new int[t.Shape[0]];
            for (var i = 0; i < permute.Length; i++) permute[i] = i + 1;

            EchelonForm<SafeDivisionWrapper<T, TWrapper>, WrapperSafeDivisionWrapper<T, TWrapper>>
                .InnerGaussianEliminationSimplePermutation(res, 0, ref permute);

            return (res.SafeDivisionToSimple(), permute);
        }

        #endregion

        #region Row echelon form leading ones

        private static GenTensor<T, TWrapper> InnerRowEchelonFormLeadingOnes(GenTensor<T, TWrapper> t)
        {
            var rowForm = t.Copy(copyElements: false);
            InnerGaussianEliminationSimpleDiscardSwapCount(rowForm, 0);
            for (int r = 0; r < t.Shape[0]; r++)
                if (rowForm.RowGetLeadingElement(r) is { } leading)
                    rowForm.RowMultiply(r, default(TWrapper).Divide(default(TWrapper).CreateOne(), leading.value));
            return rowForm;
        }

        public static GenTensor<T, TWrapper> RowEchelonFormLeadingOnesSimple(GenTensor<T, TWrapper> t)
        {
#if ALLOW_EXCEPTIONS
            if (!t.IsMatrix)
                throw new InvalidShapeException("this should be matrix");
#endif
            return InnerRowEchelonFormLeadingOnes(t);
        }

        public static GenTensor<T, TWrapper> RowEchelonFormLeadingOnesSafeDivision(GenTensor<T, TWrapper> t)
        {
#if ALLOW_EXCEPTIONS
            if (!t.IsMatrix)
                throw new InvalidShapeException("this should be matrix");
#endif
            return EchelonForm<SafeDivisionWrapper<T, TWrapper>, WrapperSafeDivisionWrapper<T, TWrapper>>
                .InnerRowEchelonFormLeadingOnes(t.SimpleToSafeDivision()).SafeDivisionToSimple();
        }

        #endregion

        #region Reduced row echelon form

        private static GenTensor<T, TWrapper> InnerReducedRowEchelonForm(GenTensor<T, TWrapper> t, out int swapCount)
        {
            var upper = t.Copy(copyElements: false);
            swapCount = 0;
            InnerGaussianEliminationSimple(upper, 0, ref swapCount);
            for (int r = t.Shape[0] - 1; r >= 0; r--)
            {
                if (upper.RowGetLeadingElement(r) is not { } leading)
                    continue;
                for (int i = 0; i < r; i++)
                    upper.RowSubtract(i, r,
                        default(TWrapper).Divide(upper.GetValueNoCheck(i, leading.index), leading.value));

                upper.RowMultiply(r, default(TWrapper).Divide(default(TWrapper).CreateOne(), leading.value));
            }

            return upper;
        }

        private static GenTensor<T, TWrapper> InnerReducedRowEchelonFormPermute(GenTensor<T, TWrapper> t,
            out int[] permutation)
        {
            var upper = t.Copy(copyElements: false);
            permutation = new int[t.Shape[0]];
            for (int i = 0; i < t.Shape[0]; i++)
            {
                permutation[i] = i + 1;
            }

            InnerGaussianEliminationSimplePermutation(upper, 0, ref permutation);
            for (int r = t.Shape[0] - 1; r >= 0; r--)
            {
                if (upper.RowGetLeadingElement(r) is not { } leading)
                    continue;
                for (int i = 0; i < r; i++)
                    upper.RowSubtract(i, r,
                        default(TWrapper).Divide(upper.GetValueNoCheck(i, leading.index), leading.value));

                upper.RowMultiply(r, default(TWrapper).Divide(default(TWrapper).CreateOne(), leading.value));
            }

            return upper;
        }

        public static GenTensor<T, TWrapper> ReducedRowEchelonFormSimple(GenTensor<T, TWrapper> t)
        {
#if ALLOW_EXCEPTIONS
            if (!t.IsMatrix)
                throw new InvalidShapeException("this should be matrix");
#endif
            return InnerReducedRowEchelonForm(t, out _);
        }

        public static GenTensor<T, TWrapper> ReducedRowEchelonFormSafeDivision(GenTensor<T, TWrapper> t)
        {
#if ALLOW_EXCEPTIONS
            if (!t.IsMatrix)
                throw new InvalidShapeException("this should be matrix");
#endif
            return EchelonForm<SafeDivisionWrapper<T, TWrapper>, WrapperSafeDivisionWrapper<T, TWrapper>>
                .InnerReducedRowEchelonForm(t.SimpleToSafeDivision(), out var _).SafeDivisionToSimple();
        }

        public static (GenTensor<T, TWrapper>, int[]) ReducedRowEchelonFormPermuteSafeDivision(GenTensor<T, TWrapper> t)
        {
#if ALLOW_EXCEPTIONS
            if (!t.IsMatrix)
                throw new InvalidShapeException("this should be matrix");
#endif
            return (EchelonForm<SafeDivisionWrapper<T, TWrapper>, WrapperSafeDivisionWrapper<T, TWrapper>>
                    .InnerReducedRowEchelonFormPermute(t.SimpleToSafeDivision(), out var result).SafeDivisionToSimple(),
                result);
        }

        #endregion
    }
}