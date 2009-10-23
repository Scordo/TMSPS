using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace TMSPS.Core.Common
{
    [Serializable]
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(Expression<Func<T>> propExpr, Expression<Func<T>> fieldExpr, T value )
        {
            SetProperty(propExpr, fieldExpr, value, ()=> { });
        }

        protected void SetProperty <T>(Expression<Func<T>> propertyExpression, Expression<Func<T>> fieldExpression, T value, Action valueChangedAction )
        {
            PropertyInfo propertyInfo = (PropertyInfo)((MemberExpression)propertyExpression.Body).Member;
            FieldInfo fieldInfo = (FieldInfo)((MemberExpression)fieldExpression.Body).Member;

            var currentValue = propertyInfo.GetValue( this, null );

            if( currentValue == null && value == null )
                return;

            if (currentValue != null && currentValue.Equals(value)) 
                return;

            fieldInfo.SetValue( this, value );
            valueChangedAction();

            if(PropertyChanged != null )
                PropertyChanged( this, new PropertyChangedEventArgs( propertyInfo.Name ) );
        }
    }
}
