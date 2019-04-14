namespace xbuffer
{
    public static class t_AuthorInfo9Buffer
    {
        public static t_AuthorInfo9 deserialize(byte[] buffer, ref uint offset)
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
			// money

			float _money = floatBuffer.deserialize(buffer, ref offset);
			// hashouse

			bool _hashouse = boolBuffer.deserialize(buffer, ref offset);
			// pbutctime

			long _pbutctime = longBuffer.deserialize(buffer, ref offset);
			// luckynumber


			int _luckynumber_length = intBuffer.deserialize(buffer, ref offset);
            int[] _luckynumber = new int[_luckynumber_length];
            for (int i = 0; i < _luckynumber_length; i++)
            {
                _luckynumber[i] = intBuffer.deserialize(buffer, ref offset);
            }

			// value
			return new t_AuthorInfo9() {
				id = _id,
				author = _author,
				age = _age,
				money = _money,
				hashouse = _hashouse,
				pbutctime = _pbutctime,
				luckynumber = _luckynumber,
            };
        }

        public static void serialize(t_AuthorInfo9 value, XSteam steam)
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
			// money

			floatBuffer.serialize(value.money, steam);
			// hashouse

			boolBuffer.serialize(value.hashouse, steam);
			// pbutctime

			longBuffer.serialize(value.pbutctime, steam);
			// luckynumber


            intBuffer.serialize(value.luckynumber.Length, steam);
            for (int i = 0; i < value.luckynumber.Length; i++)
            {
                intBuffer.serialize(value.luckynumber[i], steam);
            }
        }
    }
}
