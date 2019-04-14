namespace xbuffer
{
    public static class #CLASS_NAME#Buffer
    {
        public static #CLASS_NAME# deserialize(byte[] buffer, ref uint offset)
        {
#IF_DESERIALIZE_CLASS#
            // null
            bool _null = boolBuffer.deserialize(buffer, ref offset);
            if (_null) return null;
#END_DESERIALIZE_CLASS#
#DESERIALIZE_PROCESS#
			// #VAR_NAME#
#IF_SINGLE#
			#VAR_TYPE# _#VAR_NAME# = #VAR_TYPE#Buffer.deserialize(buffer, ref offset);
#END_SINGLE#
#IF_ARRAY#
			int _#VAR_NAME#_length = intBuffer.deserialize(buffer, ref offset);
            #VAR_TYPE#[] _#VAR_NAME# = new #VAR_TYPE#[_#VAR_NAME#_length];
            for (int i = 0; i < _#VAR_NAME#_length; i++)
            {
                _#VAR_NAME#[i] = #VAR_TYPE#Buffer.deserialize(buffer, ref offset);
            }
#END_ARRAY#
#DESERIALIZE_PROCESS#

			// value
			return new #CLASS_NAME#() {
#DESERIALIZE_RETURN#
				#VAR_NAME# = _#VAR_NAME#,
#DESERIALIZE_RETURN#
            };
        }

        public static void serialize(#CLASS_NAME# value, XSteam steam)
        {
#IF_SERIALIZE_CLASS#
            // null
            boolBuffer.serialize(value == null, steam);
            if (value == null) return;
#END_SERIALIZE_CLASS#
#SERIALIZE_PROCESS#
			// #VAR_NAME#
#IF_SINGLE#
			#VAR_TYPE#Buffer.serialize(value.#VAR_NAME#, steam);
#END_SINGLE#
#IF_ARRAY#
            intBuffer.serialize(value.#VAR_NAME#.Length, steam);
            for (int i = 0; i < value.#VAR_NAME#.Length; i++)
            {
                #VAR_TYPE#Buffer.serialize(value.#VAR_NAME#[i], steam);
            }
#END_ARRAY#
#SERIALIZE_PROCESS#
        }
    }
}
