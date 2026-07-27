namespace TKXSBASE
{
    public class yy_buffer_state
    {
        /* Number of characters read into yy_ch_buf, not including EOB
	 * characters.
	 */
        public int yy_n_chars;


        public int yy_bs_lineno; /**< The line count. */
        public int yy_bs_column; /**< The column count. */

        /* Whether this is an "interactive" input source; if so, and
         * if we're using stdio for input, then we want to use getc()
         * instead of fread(), to make sure we stop fetching input after
         * each newline.
         */
        public int yy_is_interactive;
        public char[] yy_ch_buf;        /* input buffer */
        public char[] yy_buf_pos;       /* current position in input buffer */

        /* Whether to try to fill the input buffer when we reach the
	 * end of it.
	 */
        public int yy_fill_buffer;

        public Stream yy_input_file;
    }
}