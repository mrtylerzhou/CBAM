﻿/*
 * Copyright 2017 Stanislav Muhametsin. All rights Reserved.
 *
 * Licensed  under the  Apache License,  Version 2.0  (the "License");
 * you may not use  this file  except in  compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed  under the  License is distributed on an "AS IS" BASIS,
 * WITHOUT  WARRANTIES OR CONDITIONS  OF ANY KIND, either  express  or
 * implied.
 *
 * See the License for the specific language governing permissions and
 * limitations under the License. 
 */
using CBAM.SQL.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilPack;


namespace CBAM.SQL.Implementation
{
   public abstract class StatementBuilderImpl<TParameter> : StatementBuilder
      where TParameter : StatementParameter
   {
      private readonly List<TParameter[]> _batchParameters;
      private readonly TParameter[] _currentParameters;

      public StatementBuilderImpl(
         String sql,
         Int32 parameterCount
         )
      {
         this.SQL = ArgumentValidator.ValidateNotEmpty( nameof( sql ), sql );
         this.SQLParameterCount = parameterCount;
         this._currentParameters = parameterCount > 0 ?
            new TParameter[parameterCount] :
            Empty<TParameter>.Array;
         this._batchParameters = new List<TParameter[]>();
      }


      public Int32 SQLParameterCount { get; }

      public String SQL { get; }

      public Int32 BatchParameterCount
      {
         get
         {
            return this._batchParameters.Count;
         }
      }

      public void AddBatch()
      {
         if ( this._batchParameters.Count > 0 )
         {
            // Must verify batch parameters
            var prevRow = this._batchParameters[this._batchParameters.Count - 1];
            for ( var i = 0; i < this._currentParameters.Length; ++i )
            {
               var exc = this.VerifyBatchParameters( prevRow[i], this._currentParameters[i] );
               if ( exc != null )
               {
                  throw exc;
               }
            }
         }
         this._batchParameters.Add( this._currentParameters.CreateArrayCopy() );
         Array.Clear( this._currentParameters, 0, this._currentParameters.Length );
      }

      public StatementParameter GetParameterInfo( Int32 parameterIndex )
      {
         return this._currentParameters[parameterIndex];
      }

      public StatementParameter GetBatchParameterInfo( Int32 batchIndex, Int32 parameterIndex )
      {
         return this._batchParameters[batchIndex][parameterIndex];
      }

      public void SetParameterObjectWithType( Int32 parameterIndex, Object value, Type clrType )
      {
         this._currentParameters[parameterIndex] = this.CreateStatementParameter( parameterIndex, value, clrType );
      }

      protected abstract TParameter CreateStatementParameter( Int32 parameterIndex, Object value, Type clrType );

      protected abstract SQLException VerifyBatchParameters( TParameter previous, TParameter toBeAdded );

   }

   public class StatementParameterImpl : StatementParameter
   {
      public StatementParameterImpl(
         Type cilType,
         Object value
         )
      {
         this.ParameterCILType = ArgumentValidator.ValidateNotNull( nameof( cilType ), cilType );
         this.ParameterValue = value;
      }

      public Type ParameterCILType { get; }

      public Object ParameterValue { get; }

      //public Boolean Equals( StatementParameter other )
      //{
      //   return ReferenceEquals( this, other )
      //      || ( other != null
      //      && Equals( this.ParameterCILType, other.ParameterCILType )
      //      && Equals( this.ParameterValue, other.ParameterValue )
      //      );
      //}
   }
}
