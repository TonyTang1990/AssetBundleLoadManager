namespace xbuffer
{
    public static class t_uiBuffer
    {
        public static t_ui deserialize(byte[] buffer, ref uint offset)
        {

            // null
            bool _null = boolBuffer.deserialize(buffer, ref offset);
            if (_null) return null;

			// WinName

			string _WinName = stringBuffer.deserialize(buffer, ref offset);
			// ResPath

			string _ResPath = stringBuffer.deserialize(buffer, ref offset);
			// IsFullScreen

			bool _IsFullScreen = boolBuffer.deserialize(buffer, ref offset);
			// Layer

			int _Layer = intBuffer.deserialize(buffer, ref offset);

			// value
			return new t_ui() {
				WinName = _WinName,
				ResPath = _ResPath,
				IsFullScreen = _IsFullScreen,
				Layer = _Layer,
            };
        }

        public static void serialize(t_ui value, XSteam steam)
        {

            // null
            boolBuffer.serialize(value == null, steam);
            if (value == null) return;

			// WinName

			stringBuffer.serialize(value.WinName, steam);
			// ResPath

			stringBuffer.serialize(value.ResPath, steam);
			// IsFullScreen

			boolBuffer.serialize(value.IsFullScreen, steam);
			// Layer

			intBuffer.serialize(value.Layer, steam);
        }
    }
}
