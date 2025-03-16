namespace Services.Constants
{
    public class ApplicationConstants
    {
        public const string KEYID_EXISTED = "KeyId {0} đã tồn tại.";
        public const string KeyId = "KeyId";
        public const string DUPLICATE = "Symtem_id is duplicated";
    }

    public class ResponseCodeConstants
    {
        public const string NOT_FOUND = "Not found!";
        public const string BAD_REQUEST = "Bad request!";
        public const string SUCCESS = "Success!";
        public const string FAILED = "Failed!";
        public const string EXISTED = "Existed!";
        public const string DUPLICATE = "Duplicate!";
        public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
        public const string INVALID_INPUT = "Invalid input!";
        public const string UNAUTHORIZED = "Unauthorized!";
        public const string FORBIDDEN = "Forbidden!";
        public const string EXPIRED = "Expired!";
        public const string NOTHINGCHANGED = "Nothing changed!";
    }

    public class ResponseMessageConstantsCommon
    {
        public const string NOT_FOUND = "Không tìm thấy dữ liệu";
        public const string EXISTED = "Already existed!";
        public const string SUCCESS = "Thao tác thành công";
        public const string NO_DATA = "Không có dữ liệu trả về";
        public const string SERVER_ERROR = "Lỗi từ phía server vui lòng liên hệ đội ngũ phát triển";
        public const string DATE_WRONG_FORMAT = "Dữ liệu ngày không đúng định dạng yyyy-mm-dd";
        public const string DATA_NOT_ENOUGH = "Dữ liệu đưa vào không đầy đủ";
        public const string DECIMAL_INVALID = "Vui lòng nhập giá trị lớn hơn 0";
    }

    public class ResponseMessageIdentity
    {
        public const string INVALID_USER = "Người dùng không tồn tại.";
        public const string UNAUTHENTICATED = "Không xác thực.";
        public const string USERNAME_EXISTED = "Tên người dùng đã tồn tại.";
        public const string PASSWORD_NOT_MATCH = "Mật khẩu không giống nhau.";
        public const string PASSWORD_WRONG = "Mật khẩu không đúng.";
        public const string USER_EXISTED = "Người dùng đã tồn tại.";
        public const string EMAIL_EXISTED = "Email đã tồn tại.";
        public const string PHONE_EXISTED = "Số điện thoại đã tồn tại.";
        public const string TOKEN_INVALID = "Token không xác thực.";
        public const string TOKEN_EXPIRED = "Token không xác thực hoặc đã hết hạn.";
        public const string TOKEN_INVALID_OR_EXPIRED = "Token không xác thực hoặc đã hết hạn.";
        public const string GOOGLE_TOKEN_INVALID = "Token Google không hợp lệ.";
        public const string EMAIL_VALIDATED = "Email đã được xác thực.";
        public const string PHONE_VALIDATED = "Số điện thoại đã được xác thực.";
        public const string ROLE_INVALID = "Role không xác thực.";
        public const string CLAIM_NOTFOUND = "Không tìm thấy claim.";
        public const string EXISTED_ROLE = "Role đã tồn tại.";

        public const string USERNAME_REQUIRED = "Tên người dùng không được để trống.";
        public const string NAME_REQUIRED = "Tên không được để trống.";
        public const string USERCODE_REQUIRED = "Mã người dùng không được để trống.";
        public const string PASSWORD_REQUIRED = "Mật khẩu không được để trống.";
        public const string PASSSWORD_LENGTH = "Mật khẩu phải có ít nhất 8 ký tự.";
        public const string CONFIRM_PASSWORD_REQUIRED = "Xác nhận mật khẩu không được để trống.";
        public const string EMAIL_REQUIRED = "Email không được để trống.";
        public const string PHONENUMBER_REQUIRED = "Số điện thoại không được để trống.";
        public const string PHONENUMBER_INVALID = "Số điện thoại không hợp lệ.";
        public const string PHONENUMBER_LENGTH = "Số điện thoại phải có chính xác 10 số.";
        public const string ROLES_REQUIRED = "Role không được để trống.";
        public const string USER_NOT_ALLOWED = "Bạn không có quyền truy cập vào mục này.";
        public const string EMAIL_VALIDATION_REQUIRED = "Vui lòng nhập mã OTP được gửi đến email của bạn để kích hoạt tài khoản.";
    }


    public class ResponseMessageIdentitySuccess
    {
        public const string REGIST_USER_SUCCESS = "Đăng kí tài khoản thành công!";
        public const string LOGIN_SUCCESS = "Đăng nhập thành công!";
        public const string VERIFY_PHONE_SUCCESS = "Xác thực số điện thoại thành công!";
        public const string VERIFY_EMAIL_SUCCESS = "Xác thực email thành công!";
        public const string FORGOT_PASSWORD_SUCCESS = "Yêu cầu đặt lại mật khẩu thành công! Vui lòng kiểm tra email để đặt lại mật khẩu.";
        public const string RESET_PASSWORD_SUCCESS = "Cấp lại mật khẩu thành công!";
        public const string CHANGE_PASSWORD_SUCCESS = "Đổi mật khẩu thành công!";
        public const string RESEND_EMAIL_SUCCESS = "Gửi lại email xác thực thành công!";
        public const string UPDATE_USER_SUCCESS = "Cập nhật thông tin người dùng thành công!";
        public const string DELETE_USER_SUCCESS = "Xóa người dùng thành công!";
        public const string ADD_ROLE_SUCCESS = "Thêm role thành công!";
        public const string UPDATE_ROLE_SUCCESS = "Cập nhật role thành công!";
        public const string DELETE_ROLE_SUCCESS = "Xóa role thành công!";
    }

    // Response message constants for entities: not found, existed, update success, delete success
    public class ResponseMessageConstantsUser
    {
        public const string USER_NOT_FOUND = "Không tìm thấy người dùng";
        public const string USER_EXISTED = "Người dùng đã tồn tại";
        public const string ADD_USER_SUCCESS = "Thêm người dùng thành công";
        public const string UPDATE_USER_SUCCESS = "Cập nhật người dùng thành công";
        public const string DELETE_USER_SUCCESS = "Xóa người dùng thành công";
        public const string ADMIN_NOT_FOUND = "Không tìm thấy quản trị viên";
        public const string CUSTOMER_NOT_FOUND = "Không tìm thấy khách hàng";
    }

    public class ResponseMessageConstrantsProduct
    {
        public const string NUll = "Có trường hợp bị null.";
        public const string RENTPRICE_EXACT_VALUE = "Giá cho thuê phải có 7 giá trị";
        public const string UNAUTHORIZED_UPDATE_PRODUCT = "Không thể cập nhật sản phẩm thuộc sở hữu của một cửa hàng khác";
        public const string RENTPRICE_INVALID = "Giá cho thuê không được quá 30% so với giá sản phẩm";
        public const string RENTPRICE_NOT_ASCENDING = "Giá cho thuê của ngày hôm sau phải cao hơn giá thuê của ngày hôm trước";
        public const string INVALID_ALLOWRENTBEFOREDAYS = "Số ngày thuê đặt trước tối thiểu phải từ 2 đến 5 ngày.";
        public const string UNAUTHORIZED = "Người dùng này chưa được phép để tạo sản phẩm.";
        public const string SHOP_NOTFOUND = "Cửa hàng chưa tồn tại.";
        public const string EXISTED_PRODUCTNAME = "Tên sản phẩm đã tồn tại.";
        public const string NOTFOUND = "Sản phẩm không tồn tại.";
        public const string SHOP_UNVERIFIED = "Cửa hàng chưa được xác thực.";
        public const string EXISTED_IMAGE = "Hình ảnh của sản phẩm đã được sử dụng.";
        public const string NONEXISTENT_CATEGORY = "Loại sản phẩm không tồn tại.";
        public const string NOTHING_CHANGED = "Không có gì được thay đổi sau lần chỉnh sửa này.";
        public const string NOT_EXISTED = "Danh sách có sản phẩm không tồn tại";
    }

    public class ResponseMessageConstrantsCategory
    {
        public const string NOTHING_CHANGED = "Không có gì được thay đổi sau lần chỉnh sửa này.";
        public const string EXISTED_CATEGORYNAME = "Loại sản phẩm này đã tồn tại.";
        public const string NOTFOUND = "Không tìm thấy loại sản phẩm.";
        public const string EXISTED_DESCRIPTION = "Mô tả bị trùng với một loại sản phẩm khác.";
    }

    public class ResponseMessageConstrantsOrder
    {
        public const string NOT_FOUND = "Không tìm thấy đơn hàng";
        public const string INVALID_DATA = "Dữ liệu không hợp lệ";
        public const string NOTALLOWED = "Bạn không được phép xem đơn hàng này";
        public const string CUSTOMER_NOTALLOWED = "Bạn chỉ được phép xem đơn hàng của chính mình";
        public const string CUSTOMER_NOTALLOWED_ORDER = "Bạn không được phép đặt hàng từ chính shop mình";
        public const string SHOPOWNER_NOTALLOWED = "Bạn chỉ được phép xem đơn hàng của shop bạn";
        public const string STATUS_CHANGE_NOTALLOWED = "Bạn chưa thể thay đổi trạng thái của đơn hàng này";
        public const string ALREADY_PAID = "Đơn hàng đã được thanh toán";
        public const string NOT_PAID = "Đơn hàng chưa được thanh toán";
        public const string FEEDBACK_NOTALLOWED = "Bạn chưa thể đánh giá đơn hàng này. ";
        public const string FEEDBACK_QUERY = "Bạn có 3 phương án: Lấy tất cả, Lấy theo shop hoặc Lấy theo sản phẩm";
        public const string FEEDBACK_EXISTED = "Đã đánh giá đơn hàng này";
        public const string FEEDBACK_NOTFOUND = "Không tìm thấy đánh giá";
    }

    public class ResponseMessageConstrantsImage
    {
        public const string INVALID_IMAGE = "Hình ảnh không hợp lệ. ";
        public const string INVALID_SIZE = "Kích thước hình ảnh không hợp lệ. ";
        public const string INVALID_FORMAT = "Định dạng hình ảnh không hợp lệ. ";
        public const string INVALID_URL = "Đường dẫn hình ảnh không hợp lệ. ";
    }

    public class ResponseMessageConstraintsChat
    {
        public const string NOT_FOUND = "Không tìm thấy tin nhắn";
        public const string FORBIDDEN_UPDATE = "Bạn không có quyền chỉnh sửa tin nhắn này";
        public const string FORBIDDEN_DELETE = "Bạn không có quyền xóa tin nhắn này";
    }
}