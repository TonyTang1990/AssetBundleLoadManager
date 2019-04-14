namespace xbuffer
{
    public static class t_author_InfoBuffer
    {
        public static t_author_Info deserialize(byte[] buffer, ref uint offset)
        {

            // null
            bool _null = boolBuffer.deserialize(buffer, ref offset);
            if (_null) return null;

			// id

			int _id = intBuffer.deserialize(buffer, ref offset);
			// author

			string _author = stringBuffer.deserialize(buffer, ref offset);
			// age

			int _age = intBuffer.deserialize(buffer, ref offset);
			// sex

			string _sex = stringBuffer.deserialize(buffer, ref offset);
			// national

			string _national = stringBuffer.deserialize(buffer, ref offset);

			// value
			return new t_author_Info() {
				id = _id,
				author = _author,
				age = _age,
				sex = _sex,
				national = _national,
            };
        }

        public static void serialize(t_author_Info value, XSteam steam)
        {

            // null
            boolBuffer.serialize(value == null, steam);
            if (value == null) return;

			// id

			intBuffer.serialize(value.id, steam);
			// author

			stringBuffer.serialize(value.author, steam);
			// age

			intBuffer.serialize(value.age, steam);
			// sex

			stringBuffer.serialize(value.sex, steam);
			// national

			stringBuffer.serialize(value.national, steam);
        }
    }
}
