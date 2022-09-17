﻿namespace System.Compiler
{
    using System;

    internal enum OpCode
    {
        [CLSCompliant(false)]
        _Catch = 0xfefc,
        [CLSCompliant(false)]
        _EndFilter = 0xfefb,
        [CLSCompliant(false)]
        _EndHandler = 0xfeff,
        [CLSCompliant(false)]
        _EndTry = 0xfef9,
        [CLSCompliant(false)]
        _Fault = 0xfefe,
        [CLSCompliant(false)]
        _Filter = 0xfefa,
        [CLSCompliant(false)]
        _Finally = 0xfefd,
        [CLSCompliant(false)]
        _Locals = 0xfef7,
        [CLSCompliant(false)]
        _Try = 0xfef8,
        Add = 0x58,
        Add_Ovf = 0xd6,
        Add_Ovf_Un = 0xd7,
        And = 0x5f,
        Arglist = 0xfe00,
        Beq = 0x3b,
        Beq_S = 0x2e,
        Bge = 60,
        Bge_S = 0x2f,
        Bge_Un = 0x41,
        Bge_Un_S = 0x34,
        Bgt = 0x3d,
        Bgt_S = 0x30,
        Bgt_Un = 0x42,
        Bgt_Un_S = 0x35,
        Ble = 0x3e,
        Ble_S = 0x31,
        Ble_Un = 0x43,
        Ble_Un_S = 0x36,
        Blt = 0x3f,
        Blt_S = 50,
        Blt_Un = 0x44,
        Blt_Un_S = 0x37,
        Bne_Un = 0x40,
        Bne_Un_S = 0x33,
        Box = 140,
        Br = 0x38,
        Br_S = 0x2b,
        Break = 1,
        Brfalse = 0x39,
        Brfalse_S = 0x2c,
        Brtrue = 0x3a,
        Brtrue_S = 0x2d,
        Call = 40,
        Calli = 0x29,
        Callvirt = 0x6f,
        Castclass = 0x74,
        Ceq = 0xfe01,
        Cgt = 0xfe02,
        Cgt_Un = 0xfe03,
        Ckfinite = 0xc3,
        Clt = 0xfe04,
        Clt_Un = 0xfe05,
        Constrained_ = 0xfe16,
        Conv_I = 0xd3,
        Conv_I1 = 0x67,
        Conv_I2 = 0x68,
        Conv_I4 = 0x69,
        Conv_I8 = 0x6a,
        Conv_Ovf_I = 0xd4,
        Conv_Ovf_I_Un = 0x8a,
        Conv_Ovf_I1 = 0xb3,
        Conv_Ovf_I1_Un = 130,
        Conv_Ovf_I2 = 0xb5,
        Conv_Ovf_I2_Un = 0x83,
        Conv_Ovf_I4 = 0xb7,
        Conv_Ovf_I4_Un = 0x84,
        Conv_Ovf_I8 = 0xb9,
        Conv_Ovf_I8_Un = 0x85,
        Conv_Ovf_U = 0xd5,
        Conv_Ovf_U_Un = 0x8b,
        Conv_Ovf_U1 = 180,
        Conv_Ovf_U1_Un = 0x86,
        Conv_Ovf_U2 = 0xb6,
        Conv_Ovf_U2_Un = 0x87,
        Conv_Ovf_U4 = 0xb8,
        Conv_Ovf_U4_Un = 0x88,
        Conv_Ovf_U8 = 0xba,
        Conv_Ovf_U8_Un = 0x89,
        Conv_R_Un = 0x76,
        Conv_R4 = 0x6b,
        Conv_R8 = 0x6c,
        Conv_U = 0xe0,
        Conv_U1 = 210,
        Conv_U2 = 0xd1,
        Conv_U4 = 0x6d,
        Conv_U8 = 110,
        Cpblk = 0xfe17,
        Cpobj = 0x70,
        Div = 0x5b,
        Div_Un = 0x5c,
        Dup = 0x25,
        Endfilter = 0xfe11,
        Endfinally = 220,
        Initblk = 0xfe18,
        Initobj = 0xfe15,
        Isinst = 0x75,
        Jmp = 0x27,
        Ldarg = 0xfe09,
        Ldarg_0 = 2,
        Ldarg_1 = 3,
        Ldarg_2 = 4,
        Ldarg_3 = 5,
        Ldarg_S = 14,
        Ldarga = 0xfe0a,
        Ldarga_S = 15,
        Ldc_I4 = 0x20,
        Ldc_I4_0 = 0x16,
        Ldc_I4_1 = 0x17,
        Ldc_I4_2 = 0x18,
        Ldc_I4_3 = 0x19,
        Ldc_I4_4 = 0x1a,
        Ldc_I4_5 = 0x1b,
        Ldc_I4_6 = 0x1c,
        Ldc_I4_7 = 0x1d,
        Ldc_I4_8 = 30,
        Ldc_I4_M1 = 0x15,
        Ldc_I4_S = 0x1f,
        Ldc_I8 = 0x21,
        Ldc_R4 = 0x22,
        Ldc_R8 = 0x23,
        Ldelem = 0xa3,
        Ldelem_I = 0x97,
        Ldelem_I1 = 0x90,
        Ldelem_I2 = 0x92,
        Ldelem_I4 = 0x94,
        Ldelem_I8 = 150,
        Ldelem_R4 = 0x98,
        Ldelem_R8 = 0x99,
        Ldelem_Ref = 0x9a,
        Ldelem_U1 = 0x91,
        Ldelem_U2 = 0x93,
        Ldelem_U4 = 0x95,
        Ldelema = 0x8f,
        Ldfld = 0x7b,
        Ldflda = 0x7c,
        Ldftn = 0xfe06,
        Ldind_I = 0x4d,
        Ldind_I1 = 70,
        Ldind_I2 = 0x48,
        Ldind_I4 = 0x4a,
        Ldind_I8 = 0x4c,
        Ldind_R4 = 0x4e,
        Ldind_R8 = 0x4f,
        Ldind_Ref = 80,
        Ldind_U1 = 0x47,
        Ldind_U2 = 0x49,
        Ldind_U4 = 0x4b,
        Ldlen = 0x8e,
        Ldloc = 0xfe0c,
        Ldloc_0 = 6,
        Ldloc_1 = 7,
        Ldloc_2 = 8,
        Ldloc_3 = 9,
        Ldloc_S = 0x11,
        Ldloca = 0xfe0d,
        Ldloca_S = 0x12,
        Ldnull = 20,
        Ldobj = 0x71,
        Ldsfld = 0x7e,
        Ldsflda = 0x7f,
        Ldstr = 0x72,
        Ldtoken = 0xd0,
        Ldvirtftn = 0xfe07,
        Leave = 0xdd,
        Leave_S = 0xde,
        Localloc = 0xfe0f,
        Mkrefany = 0xc6,
        Mul = 90,
        Mul_Ovf = 0xd8,
        Mul_Ovf_Un = 0xd9,
        Neg = 0x65,
        Newarr = 0x8d,
        Newobj = 0x73,
        Nop = 0,
        Not = 0x66,
        Or = 0x60,
        Pop = 0x26,
        Prefix1 = 0xfe,
        Prefix2 = 0xfd,
        Prefix3 = 0xfc,
        Prefix4 = 0xfb,
        Prefix5 = 250,
        Prefix6 = 0xf9,
        Prefix7 = 0xf8,
        PrefixRef = 0xff,
        Readonly_ = 0xfe1e,
        Refanytype = 0xfe1d,
        Refanyval = 0xc2,
        Rem = 0x5d,
        Rem_Un = 0x5e,
        Ret = 0x2a,
        Rethrow = 0xfe1a,
        Shl = 0x62,
        Shr = 0x63,
        Shr_Un = 100,
        Sizeof = 0xfe1c,
        Starg = 0xfe0b,
        Starg_S = 0x10,
        Stelem = 0xa4,
        Stelem_I = 0x9b,
        Stelem_I1 = 0x9c,
        Stelem_I2 = 0x9d,
        Stelem_I4 = 0x9e,
        Stelem_I8 = 0x9f,
        Stelem_R4 = 160,
        Stelem_R8 = 0xa1,
        Stelem_Ref = 0xa2,
        Stfld = 0x7d,
        Stind_I = 0xdf,
        Stind_I1 = 0x52,
        Stind_I2 = 0x53,
        Stind_I4 = 0x54,
        Stind_I8 = 0x55,
        Stind_R4 = 0x56,
        Stind_R8 = 0x57,
        Stind_Ref = 0x51,
        Stloc = 0xfe0e,
        Stloc_0 = 10,
        Stloc_1 = 11,
        Stloc_2 = 12,
        Stloc_3 = 13,
        Stloc_S = 0x13,
        Stobj = 0x81,
        Stsfld = 0x80,
        Sub = 0x59,
        Sub_Ovf = 0xda,
        Sub_Ovf_Un = 0xdb,
        Switch = 0x45,
        Tail_ = 0xfe14,
        Throw = 0x7a,
        Unaligned_ = 0xfe12,
        Unbox = 0x79,
        Unbox_Any = 0xa5,
        Volatile_ = 0xfe13,
        Xor = 0x61
    }
}
