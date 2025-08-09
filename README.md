# SIMS_APDP - Student Information Management System

## Mô tả
SIMS_APDP là một hệ thống quản lý thông tin sinh viên được xây dựng bằng ASP.NET Core với Entity Framework và JWT Authentication.

## Tính năng chính

### 1. Authentication & Authorization
- **JWT Token Authentication**: Hệ thống xác thực bằng JWT tokens
- **Role-based Authorization**: Phân quyền theo vai trò (Admin, Student, Teacher)
- **Default Admin Account**: Tài khoản admin mặc định (username: admin, password: admin123)

### 2. Quản lý Sinh viên (Students)
- **CRUD Operations**: Tạo, đọc, cập nhật, xóa thông tin sinh viên
- **Student Registration**: Đăng ký sinh viên mới
- **Course Enrollment**: Xem danh sách khóa học đã đăng ký
- **Grade Management**: Quản lý điểm số và xếp loại

### 3. Quản lý Giảng viên (Teachers)
- **CRUD Operations**: Tạo, đọc, cập nhật, xóa thông tin giảng viên
- **Teacher Registration**: Đăng ký giảng viên mới
- **Course Assignment**: Gán giảng viên cho khóa học
- **Course Management**: Quản lý các khóa học được phân công

### 4. Quản lý Khóa học (Courses)
- **CRUD Operations**: Tạo, đọc, cập nhật, xóa thông tin khóa học
- **Student Enrollment**: Đăng ký sinh viên vào khóa học
- **Teacher Assignment**: Gán giảng viên cho khóa học
- **Enrollment Management**: Quản lý danh sách đăng ký

## Cấu trúc Database

### Bảng Users
- Id (Primary Key)
- Username (Unique)
- PasswordHash
- Role (Admin/Student/Teacher)
- CreatedAt

### Bảng Students
- Id (Primary Key)
- FirstName, LastName
- Email (Unique)
- StudentId (Unique)
- PhoneNumber, Address
- DateOfBirth
- UserId (Foreign Key to Users)

### Bảng Teachers
- Id (Primary Key)
- FirstName, LastName
- Email (Unique)
- TeacherId (Unique)
- PhoneNumber, Address
- Department, Specialization
- UserId (Foreign Key to Users)

### Bảng Courses
- Id (Primary Key)
- Name, Code (Unique)
- Description
- Credits
- TeacherId (Foreign Key to Teachers)
- CreatedAt

### Bảng StudentCourses (Many-to-Many)
- Id (Primary Key)
- StudentId (Foreign Key to Students)
- CourseId (Foreign Key to Courses)
- EnrolledAt
- Grade, LetterGrade

## API Endpoints

### Authentication
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/register` - Đăng ký sinh viên
- `POST /api/auth/create-admin` - Tạo admin mặc định

### Students
- `GET /api/students` - Lấy danh sách sinh viên (Admin only)
- `GET /api/students/{id}` - Lấy thông tin sinh viên (Admin only)
- `POST /api/students` - Tạo sinh viên mới (Admin only)
- `PUT /api/students/{id}` - Cập nhật sinh viên (Admin only)
- `DELETE /api/students/{id}` - Xóa sinh viên (Admin only)
- `GET /api/students/my-courses` - Xem khóa học của mình (Student only)

### Teachers
- `GET /api/teachers` - Lấy danh sách giảng viên (Admin only)
- `GET /api/teachers/{id}` - Lấy thông tin giảng viên (Admin only)
- `POST /api/teachers` - Tạo giảng viên mới (Admin only)
- `PUT /api/teachers/{id}` - Cập nhật giảng viên (Admin only)
- `DELETE /api/teachers/{id}` - Xóa giảng viên (Admin only)
- `GET /api/teachers/my-courses` - Xem khóa học được phân công (Teacher only)

### Courses
- `GET /api/courses` - Lấy danh sách khóa học (Admin only)
- `GET /api/courses/{id}` - Lấy thông tin khóa học (Admin only)
- `POST /api/courses` - Tạo khóa học mới (Admin only)
- `PUT /api/courses/{id}` - Cập nhật khóa học (Admin only)
- `DELETE /api/courses/{id}` - Xóa khóa học (Admin only)
- `POST /api/courses/enroll` - Đăng ký sinh viên vào khóa học (Admin only)
- `GET /api/courses/enrollments` - Lấy danh sách đăng ký (Admin only)
- `GET /api/courses/{courseId}/students` - Lấy danh sách sinh viên trong khóa học (Admin only)
- `DELETE /api/courses/enrollments/{id}` - Hủy đăng ký (Admin only)
- `POST /api/courses/{id}/assign-teacher` - Gán giảng viên cho khóa học (Admin only)

## Cài đặt và Chạy

### Yêu cầu hệ thống
- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 hoặc VS Code

### Bước 1: Clone repository
```bash
git clone <repository-url>
cd SIMS_APDP
```

### Bước 2: Cập nhật connection string
Chỉnh sửa file `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SIMS_APDP;User Id=sa;Password=Thang2005@;TrustServerCertificate=true;"
  }
}
```

### Bước 3: Restore packages
```bash
dotnet restore
```

### Bước 4: Tạo database
```bash
dotnet ef database update
```

### Bước 5: Chạy ứng dụng
```bash
dotnet run
```

## Tài khoản mặc định
- **Username**: admin
- **Password**: admin123
- **Role**: Admin

## Cấu trúc Project

```
SIMS_APDP/
├── Controllers/
│   ├── AuthController.cs
│   ├── StudentsController.cs
│   ├── TeachersController.cs
│   └── CoursesController.cs
├── Data/
│   └── SIMSContext.cs
├── DTOs/
│   ├── AuthDTOs.cs
│   ├── StudentDTOs.cs
│   ├── TeacherDTOs.cs
│   └── CourseDTOs.cs
├── Models/
│   ├── User.cs
│   ├── Student.cs
│   ├── Teacher.cs
│   ├── Course.cs
│   └── StudentCourse.cs
├── Services/
│   └── AuthService.cs
├── Views/
├── wwwroot/
├── Program.cs
├── appsettings.json
└── SIMS_APDP.csproj
```

## Bảo mật
- Sử dụng BCrypt để hash password
- JWT tokens với expiration time
- Role-based authorization
- Input validation và sanitization
- SQL injection protection thông qua Entity Framework

## Tương lai
- Thêm giao diện web (Razor Pages hoặc React/Angular)
- Thêm tính năng upload file (ảnh đại diện, tài liệu)
- Thêm tính năng báo cáo và thống kê
- Thêm tính năng notification
- Thêm tính năng backup và restore database
