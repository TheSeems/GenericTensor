﻿#region copyright
/*
 * MIT License
 * 
 * Copyright (c) 2020 WhiteBlackGoose
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


using System;
using System.Runtime.CompilerServices;
using GenericTensor.Functions;

namespace GenericTensor.Core
{
    public partial class GenTensor<T>
    {
        /// <summary>
        /// Creates a tensor whose all matrices are identity matrices
        /// <para>1 is achieved with <see cref="ConstantsAndFunctions{T}.CreateOne"/></para>
        /// <para>0 is achieved with <see cref="ConstantsAndFunctions{T}.CreateZero"/></para>
        /// </summary>
        public static GenTensor<T> CreateIdentityTensor(int[] dimensions, int finalMatrixDiag)
        {
            var newDims = new int[dimensions.Length + 2];
            for (int i = 0; i < dimensions.Length; i++)
                newDims[i] = dimensions[i];
            newDims[newDims.Length - 2] = newDims[newDims.Length - 1] = finalMatrixDiag;
            var res = new GenTensor<T>(newDims);
            foreach (var index in res.IterateOverMatrices())
            {
                var iden = CreateIdentityMatrix(finalMatrixDiag);
                res.SetSubtensor(iden, index);
            }
            return res;
        }

        /// <summary>
        /// Creates an indentity matrix whose width and height are equal to diag
        /// <para>1 is achieved with <see cref="ConstantsAndFunctions{T}.CreateOne"/></para>
        /// <para>0 is achieved with <see cref="ConstantsAndFunctions{T}.CreateZero"/></para>
        /// </summary>
        public static GenTensor<T> CreateIdentityMatrix(int diag)
        {
            var res = new GenTensor<T>(diag, diag);
            for (int i = 0; i < res.data.Length; i++)
                res.data[i] = ConstantsAndFunctions<T>.CreateZero();

            for (int i = 0; i < diag; i++)
                res.SetValueNoCheck(ConstantsAndFunctions<T>.CreateOne, i, i);
            return res;
        }

        /// <summary>
        /// Creates a vector from an array of primitives
        /// Its length will be equal to elements.Length
        /// </summary>
        public static GenTensor<T> CreateVector(params T[] elements)
        {
            var res = new GenTensor<T>(elements.Length);
            for (int i = 0; i < elements.Length; i++)
                res.SetValueNoCheck(elements[i], i);
            return res;
        }

        /// <summary>
        /// Creates a vector from an array of primitives
        /// Its length will be equal to elements.Length
        /// </summary>
        public static GenTensor<T> CreateVector(int length)
        {
            var res = new GenTensor<T>(length);
            return res;
        }

        private static (int height, int width) ExtractAndCheck(T[,] data)
        {
            var width = data.GetLength(0);
            #if ALLOW_EXCEPTIONS
            if (width <= 0)
                throw new InvalidShapeException();
            #endif
            var height = data.GetLength(1);
            #if ALLOW_EXCEPTIONS
            if (height <= 0)
                throw new InvalidShapeException();
            #endif
            return (width, height);
        }

        /// <summary>
        /// Creates a matrix from a two-dimensional array of primitives
        /// for example
        /// <code>
        /// var M = Tensor.CreateMatrix(new[,]
        /// {
        ///     {1, 2},
        ///     {3, 4}
        /// });
        /// </code>
        /// where yourData.GetLength(0) is Shape[0] and
        /// yourData.GetLength(1) is Shape[1]
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GenTensor<T> CreateMatrix(T[,] data)
        {
            var (width, height) = GenTensor<T>.ExtractAndCheck(data);
            var res = new GenTensor<T>(width, height);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    res.SetValueNoCheck(data[x, y], x, y);
            return res;
        }

        /// <summary>
        /// Creates an uninitialized square matrix
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GenTensor<T> CreateSquareMatrix(int diagLength)
            => CreateMatrix(diagLength, diagLength);

        /// <summary>
        /// Creates a matrix of width and height size
        /// and iterator for each pair of coordinate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GenTensor<T> CreateMatrix(int width, int height, Func<int, int, T> stepper)
        {
            var res = GenTensor<T>.CreateMatrix(width, height);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    res.SetValueNoCheck(stepper(x, y), x, y);
            return res;
        }

        /// <summary>
        /// Creates a matrix of width and height size
        /// </summary>
        public static GenTensor<T> CreateMatrix(int width, int height)
            => new GenTensor<T>(width, height);

        /// <summary>
        /// Creates a tensor of given size with iterator over its indices
        /// (its only argument is an array of integers which are indices of the tensor)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GenTensor<T> CreateTensor(TensorShape shape, Func<int[], T> operation)
        {
            var res = new GenTensor<T>(shape);
            foreach (var ind in res.IterateOverElements())
                res.SetValueNoCheck(operation(ind), ind);
            return res;
        }

        /// <summary>
        /// Creates a tensor from an array
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GenTensor<T> CreateTensor(T[] data)
        {
            var res = new GenTensor<T>(data.GetLength(0));
            for (int x = 0; x < data.GetLength(0); x++)
                res.SetValueNoCheck(data[x], x);
            return res;
        }

        /// <summary>
        /// Creates a tensor from a two-dimensional array
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GenTensor<T> CreateTensor(T[,] data)
        {
            var res = new GenTensor<T>(data.GetLength(0), data.GetLength(1));
            for (int x = 0; x < data.GetLength(0); x++)
                for (int y = 0; y < data.GetLength(1); y++)
                    res.SetValueNoCheck(data[x, y], x, y);
            return res;
        }

        /// <summary>
        /// Creates a tensor from a three-dimensional array
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GenTensor<T> CreateTensor(T[,,] data)
        {
            var res = new GenTensor<T>(data.GetLength(0),
                data.GetLength(1), data.GetLength(2));
            for (int x = 0; x < data.GetLength(0); x++)
                for (int y = 0; y < data.GetLength(1); y++)
                    for (int z = 0; z < data.GetLength(2); z++)
                        res.SetValueNoCheck(data[x, y, z], x, y, z);
            return res;
        }

        /// <summary>
        /// Creates a tensor from an n-dimensional array
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GenTensor<T> CreateTensor(Array data)
        {
            var dimensions = new int[data.Rank];
            for (int i = 0; i < data.Rank; i++)
                dimensions[i] = data.GetLength(i);
            var res = new GenTensor<T>(dimensions);

            dimensions = new int[data.Rank]; // Don't modify res
            var normalizedIndices = new int[data.Rank];
            var indices = new int[data.Rank];
            for (int i = 0; i < data.Rank; i++)
            {
                dimensions[i] = data.GetUpperBound(i);
                indices[i] = data.GetLowerBound(i);
            }
            var increment = indices.Length - 1;
            while (true)
            {
                for (int i = increment; indices[i] > dimensions[i]; i--)
                    if (i == 0)
                        return res;
                    else
                    {
                        indices[i - 1]++;
                        indices[i] = data.GetLowerBound(i);
                        normalizedIndices[i - 1]++;
                        normalizedIndices[i] = 0;
                    }
                res.SetValueNoCheck((T)data.GetValue(indices), normalizedIndices);
                indices[increment]++;
                normalizedIndices[increment]++;
            }
        }
    }
}
