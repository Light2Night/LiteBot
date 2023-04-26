namespace DiscordBot {
	public static class TypeChecker {
		public static bool IsUInt32<T>(T number) {
			try {
				Convert.ToUInt32(number);
				return true;
			}
			catch (Exception) { }
			return false;
		}
	}
}