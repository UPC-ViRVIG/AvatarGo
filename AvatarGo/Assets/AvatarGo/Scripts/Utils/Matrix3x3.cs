using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Matrix3x3
{
    public static Matrix3x3 identity
    {
        get
        {
            return new Matrix3x3(Matrix4x4.identity);
        }
    }

    public static Matrix3x3 zero
    {
        get
        {
            return new Matrix3x3(Matrix4x4.zero);
        }
    }

    public Matrix3x3 inverse
    {
        get
        {
            return new Matrix3x3(_mat4x4.inverse);
        }
    }

    public Matrix3x3 transpose
    {
        get
        {
            return new Matrix3x3(_mat4x4.transpose);
        }
    }

    [SerializeField]
    private Matrix4x4 _mat4x4;

    public float m00
    {
        get { return _mat4x4.m00; }
        set { _mat4x4.m00 = value; }
    }
    public float m10
    {
        get { return _mat4x4.m10; }
        set { _mat4x4.m10 = value; }
    }
    public float m20
    {
        get { return _mat4x4.m20; }
        set { _mat4x4.m20 = value; }
    }
    public float m01
    {
        get { return _mat4x4.m01; }
        set { _mat4x4.m01 = value; }
    }
    public float m11
    {
        get { return _mat4x4.m11; }
        set { _mat4x4.m11 = value; }
    }
    public float m21
    {
        get { return _mat4x4.m21; }
        set { _mat4x4.m21 = value; }
    }
    public float m02
    {
        get { return _mat4x4.m02; }
        set { _mat4x4.m02 = value; }
    }
    public float m12
    {
        get { return _mat4x4.m12; }
        set { _mat4x4.m12 = value; }
    }
    public float m22
    {
        get { return _mat4x4.m22; }
        set { _mat4x4.m22 = value; }
    }

    /// <summary>
    /// Creates a Matrix3x3 based on a Matrix4x4 with 4th row and column like an identity
    /// </summary>
    /// <param name="mat4x4"></param>
    public Matrix3x3(Matrix4x4 mat4x4)
    {
        this._mat4x4 = mat4x4;
        mat4x4.SetRow(3, new Vector4(0, 0, 0, 1));
        mat4x4.SetColumn(3, new Vector4(0, 0, 0, 1));
    }

    /// <summary>
    /// Creates a Matrix3x3 based on three column vector3.
    /// </summary>
    /// <param name="column1"></param>
    /// <param name="column2"></param>
    /// <param name="column3"></param>
    public Matrix3x3(Vector3 column1, Vector3 column2, Vector3 column3)
    {
        _mat4x4 = new Matrix4x4(
            column1,
            column2,
            column3,
            new Vector4(0, 0, 0, 1));
    }

    /// <summary>
    /// Multiplies two matrices. The returned result is lhs * rhs.
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static Matrix3x3 operator *(Matrix3x3 lhs, Matrix3x3 rhs)
    {
        return new Matrix3x3(lhs._mat4x4 * rhs._mat4x4);
    }

    /// <summary>
    /// Multiplies a matrix and a vector3. The returned result is lhs * m.
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector3 operator *(Matrix3x3 lhs, Vector3 vector)
    {
        return lhs._mat4x4 * vector;
    }

    /// <summary>
    /// Creates a scaling matrix.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Matrix3x3 Scale(Vector2 vector)
    {
        return new Matrix3x3(Matrix4x4.Scale(vector));
    }

    /// <summary>
    /// Creates a rotation matrix.
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    public static Matrix3x3 Rotate(Quaternion q)
    {
        return new Matrix3x3(Matrix4x4.Rotate(q));
    }

    /// <summary>
    /// Returns a Matrix4x4 with the same characteristics.
    /// </summary>
    /// <returns></returns>
    public Matrix4x4 ToMatrix4X4()
    {
        return _mat4x4;
    }

    /// <summary>
    /// Access values of the matrix by row and column.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public float this[int row, int column]
    {
        get
        {
            if (row > 2 || column > 2) throw new IndexOutOfRangeException();
            return _mat4x4[row, column];
        }
        set
        {
            if (row > 2 || column > 2) throw new IndexOutOfRangeException();
            _mat4x4[row, column] = value;
        }
    }

    public string ToString(string format, string padding = " ")
    {
        return this[0, 0].ToString(format) + padding + this[0, 1].ToString(format) + padding + this[0, 2].ToString(format) + padding + "\n"
               + this[1, 0].ToString(format) + padding + this[1, 1].ToString(format) + padding + this[1, 2].ToString(format) + padding + "\n"
               + this[2, 0].ToString(format) + padding + this[2, 1].ToString(format) + padding + this[2, 2].ToString(format) + padding + "\n";
    }

    public override string ToString()
    {
        return ToString("");
        //            return this[0, 0].ToString("F1") + "   " + this[0, 1].ToString("F1") + "   " + this[0, 2].ToString("F1") + "   " + "\n"
        //                + this[1, 0].ToString("F1") + "   " + this[1, 1].ToString("F1") + "   " + this[1, 2].ToString("F1") + "   " + "\n"
        //                + this[2, 0].ToString("F1") + "   " + this[2, 1].ToString("F1") + "   " + this[2, 2].ToString("F1") + "   " + "\n";
    }
}