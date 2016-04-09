using System;
using System.Reflection;

namespace VTGPost.Helper {
    public class ObjectHelper {
        // Methods
        public static void Copy(object source, object destination) {
            PropertyInfo[] sourceProperties = null;
            PropertyInfo[] destinationProperties = null;
            sourceProperties = source.GetType().GetProperties();
            destinationProperties = destination.GetType().GetProperties();
            CopyProperties(sourceProperties, destinationProperties, source, destination);
        }

        private static void CopyProperties(PropertyInfo[] sourceProperties, PropertyInfo[] destinationProperties, object source, object destination) {
            PropertyInfo info = null;
            PropertyInfo info2 = null;
            try {
                if ((sourceProperties != null) && (destinationProperties != null)) {
                    for (int i = 0; i < sourceProperties.Length; i++) {
                        info = sourceProperties[i];
                        for (int j = 0; j < destinationProperties.Length; j++) {
                            info2 = destinationProperties[j];
                            if (info.Name == info2.Name) {
                                if (!info.PropertyType.BaseType.Name.Equals("DataSet")) {
                                    info2.SetValue(destination, info.GetValue(source, null), null);
                                }
                                break;
                            }
                        }
                    }
                }
            } catch (Exception exception) {
                throw exception;
            }
        }
    }

 
}
