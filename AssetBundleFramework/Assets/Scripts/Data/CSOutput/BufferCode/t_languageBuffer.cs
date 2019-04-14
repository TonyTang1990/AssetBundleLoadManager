namespace xbuffer
{
    public static class t_languageBuffer
    {
        public static t_language deserialize(byte[] buffer, ref uint offset)
        {

            // null
            bool _null = boolBuffer.deserialize(buffer, ref offset);
            if (_null) return null;

			// id

			int _id = intBuffer.deserialize(buffer, ref offset);
			// content

			string _content = stringBuffer.deserialize(buffer, ref offset);

			// value
			return new t_language() {
				id = _id,
				content = _content,
            };
        }

        public static void serialize(t_language value, XSteam steam)
        {

            // null
            boolBuffer.serialize(value == null, steam);
            if (value == null) return;

			// id

			intBuffer.serialize(value.id, steam);
			// content

			stringBuffer.serialize(value.content, steam);
        }
    }
}
