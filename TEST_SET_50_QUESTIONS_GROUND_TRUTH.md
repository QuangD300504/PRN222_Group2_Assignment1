# PRN222 RAG Evaluation Benchmark: 50 Questions & Ground Truth Dataset

> **Dataset Description**: Standard evaluation dataset containing 50 diverse questions, human-verified ground-truth answers, source document titles, exact page/slide references, and expected citation behavior.  
> **Target Materials**:
> 1. `Chapter 01 - Networking Programming.pdf` (FPT University PRN222 Lecture Slides)
> 2. `Application programming interface.pdf` (Technical Specification & Architecture Reference)

---

## Summary Matrix

| Category | Description | Question Range |
|---|---|:---:|
| **Category 1** | Single-Document Technical Fact Extraction | Q01 – Q15 |
| **Category 2** | Cross-Lingual / Bilingual Retrieval (VI Question $\rightarrow$ EN Docs) | Q16 – Q28 |
| **Category 3** | Deep Architecture & Design Principles (API Theory) | Q29 – Q38 |
| **Category 4** | Cross-Document Synthesis & Comparison | Q39 – Q44 |
| **Category 5** | Strict Anti-Hallucination & Guardrail Refusal Tests | Q45 – Q50 |

---

## Category 1: Single-Document Technical Fact Extraction

### Q01: What is the fundamental difference between TCP and UDP regarding connection and acknowledgement?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 6, 53)
* **Ground Truth Answer**: TCP is a connection-based protocol (single connecting line) that provides reliable data flow based on an acknowledgement mechanism. In contrast, UDP is connectionless, sending independent datagram packets with no arrival guarantees and no acknowledgement mechanism.
* **Expected Citations**: `[1]` Slide 6 (Definitions), `[2]` Slide 53 (Working UDP Services)

### Q02: What is the bit length of IPv4 and IPv6 addresses according to the networking basics slide?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 5, 9)
* **Ground Truth Answer**: An IPv4 address is 4 bytes (32-bit unsigned integer), such as `192.143.5.1`. An IPv6 address is 16 bytes (128-bit unsigned integer).
* **Expected Citations**: `[1]` Slide 5

### Q03: What are the three main components that make up the Domain Name System (DNS)?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 33)
* **Ground Truth Answer**: The DNS consists of: (1) A "Name Space" defining syntactical rules for legal DNS names, (2) A "Globally Distributed Database" implemented across Name Servers, and (3) "Resolver" software that formulates DNS queries.
* **Expected Citations**: `[1]` Slide 33

### Q04: What are the primary elements into which a URI can be divided, and what .NET class exposes them?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 11)
* **Ground Truth Answer**: A URI can be broken down into `scheme`, `authority`, and `path`. In .NET, the `Uri` class in the `System` namespace exposes these elements as individual properties (e.g., Host, Port, PathAndQuery, Query, Fragment).
* **Expected Citations**: `[1]` Slide 11

### Q05: List the key properties of the `WebRequest` class in .NET.
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 19)
* **Ground Truth Answer**: `ContentLength`, `ContentType`, `Credentials`, `Method`, `Headers`, `RequestUri`, and `Timeout`.
* **Expected Citations**: `[1]` Slide 19

### Q06: What key methods are provided by the `WebRequest` class for executing HTTP/FTP requests?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 20)
* **Ground Truth Answer**: `Create(Uri)`, `GetRequestStream()`, `GetResponse()`, `CreateHttp(String)`, `BeginGetRequestStream()`, `BeginGetResponse()`, and `Abort()`.
* **Expected Citations**: `[1]` Slide 20

### Q07: How is an instance of `WebResponse` created in a client application?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 21)
* **Ground Truth Answer**: Client applications do not instantiate `WebResponse` objects directly; they are created by calling the `GetResponse()` method on a `WebRequest` instance.
* **Expected Citations**: `[1]` Slide 21

### Q08: Why was the `HttpClient` class introduced to replace `WebClient` and `WebRequest` in modern .NET?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 24)
* **Ground Truth Answer**: `HttpClient` was created in response to the growth of HTTP-based web APIs and RESTful services. It provides a higher-level abstraction, supports custom authentication, rich extensible headers, custom message handlers, and handles HTTP directly in .NET Core rather than relying on WebRequest.
* **Expected Citations**: `[1]` Slide 24

### Q09: What connection pooling behavior does an `HttpClient` instance have in .NET?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 25)
* **Ground Truth Answer**: Every `HttpClient` instance uses its own connection pool, isolating its requests from requests executed by other `HttpClient` instances. It is intended to be instantiated once per application and reused.
* **Expected Citations**: `[1]` Slide 25, 27

### Q10: What are the two ways to connect a `TcpClient` to a remote listener in .NET?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 38)
* **Ground Truth Answer**: (1) Create a `TcpClient` and call one of the three `Connect()` methods, or (2) Instantiate `TcpClient` using the constructor that accepts the host name and port number, which automatically attempts connection.
* **Expected Citations**: `[1]` Slide 38

### Q11: What methods are used in `TcpListener` to accept incoming connection requests?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 37)
* **Ground Truth Answer**: `AcceptSocket()` / `AcceptSocketAsync()` to accept requests as raw Sockets, and `AcceptTcpClient()` / `AcceptTcpClientAsync()` to accept requests as `TcpClient` instances.
* **Expected Citations**: `[1]` Slide 37

### Q12: What method is used on `TcpClient` to obtain the underlying stream for reading and writing data?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 40, 47)
* **Ground Truth Answer**: `client.GetStream()`, which returns a `NetworkStream` object used for stream read and write operations.
* **Expected Citations**: `[1]` Slide 40

### Q13: What is the port number range reserved for system and standard services in TCP/IP?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 10)
* **Ground Truth Answer**: Port numbers ranging from `0` to `1023` are reserved. The logical connection range spans from `0` to `65535`.
* **Expected Citations**: `[1]` Slide 10

### Q14: Which .NET class is used for User Datagram Protocol communication, and what are its key methods?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 53, 54)
* **Ground Truth Answer**: The `UdpClient` class in `System.Net.Sockets`. Key methods include `Connect()`, `Send()`, `Receive()`, `JoinMulticastGroup()`, `Close()`, and `Dispose()`.
* **Expected Citations**: `[1]` Slide 54

### Q15: What is the difference between a URL and a URN according to the slides?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 12, 13)
* **Ground Truth Answer**: A URL (Uniform Resource Locator) specifies the network location and protocol to access a resource. A URN (Uniform Resource Name) is a location-independent persistent identifier using the `urn:` scheme (e.g., `urn:isbn:0451450523`) that does not imply availability of the resource at a specific location.
* **Expected Citations**: `[1]` Slide 12, 13

---

## Category 2: Cross-Lingual / Bilingual Retrieval (VI Question $\rightarrow$ EN Slides)

### Q16: Giao thức TCP đảm bảo tính tin cậy của luồng truyền dữ liệu giữa hai máy tính bằng cơ chế nào?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 36)
* **Ground Truth Answer**: TCP đảm bảo luồng truyền dữ liệu tin cậy thông qua cơ chế xác nhận (acknowledgement mechanism) và thiết lập kết nối hướng liên kết (connection-based).
* **Expected Citations**: `[1]` Slide 36

### Q17: Tại sao các ứng dụng sử dụng giao thức UDP phải tự xử lý các gói tin bị mất hoặc đảo lộn thứ tự?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 53)
* **Ground Truth Answer**: Do UDP là giao thức phi kết nối (connectionless), các gói tin (datagrams) được gửi độc lập và không có cơ chế đảm bảo việc nhận gói tin hay đảm bảo thứ tự gửi đến đích.
* **Expected Citations**: `[1]` Slide 53

### Q18: Trong lập trình .NET, lớp `Dns` cung cấp phương thức nào để phân giải tên miền thành địa chỉ IP?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 16, 34)
* **Ground Truth Answer**: Phương thức `Dns.GetHostEntry()` (hoặc `Dns.GetHostAddresses()`) trong namespace `System.Net`.
* **Expected Citations**: `[1]` Slide 34

### Q19: Mô tả quy trình hoạt động giữa Server và Client trong mô hình Client-Server?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 7)
* **Ground Truth Answer**: Quy trình gồm 3 bước: (1) Client gửi request đến Server, (2) Server phân tích và xử lý request, (3) Server gửi kết quả response trả về cho Client.
* **Expected Citations**: `[1]` Slide 7

### Q20: Lớp nào trong `System.Net` đóng vai trò là lớp facade đơn giản để upload/download qua HTTP hoặc FTP?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 16, 24)
* **Ground Truth Answer**: Lớp `WebClient` (được thiết kế dưới dạng Facade pattern cho các thao tác tải lên/tải xuống cơ bản).
* **Expected Citations**: `[1]` Slide 16

### Q21: Trong mô hình TCP Server, lệnh `TcpListener.Start()` có nhiệm vụ gì?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 37, 48)
* **Ground Truth Answer**: Bắt đầu lắng nghe (listening) các yêu cầu kết nối đến từ các client trên địa chỉ IP và cổng (port) đã chỉ định.
* **Expected Citations**: `[1]` Slide 37

### Q22: Thuộc tính `Connected` trong lớp `Socket` hoặc `TcpClient` cho biết điều gì?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 39, 42)
* **Ground Truth Answer**: Trả về giá trị boolean biểu thị liệu Socket / TcpClient có đang được kết nối với host từ xa tại thời điểm thao tác Send/Receive gần nhất hay không.
* **Expected Citations**: `[1]` Slide 39 / Slide 42

### Q23: Mục đích của việc thêm Header vào gói tin khi truyền qua các tầng mạng là gì?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 8)
* **Ground Truth Answer**: Header chứa dữ liệu định danh (identifiable data như Port cho Transport Layer, IP cho Network Layer) giúp các tầng giao thức định tuyến và xử lý chính xác gói tin.
* **Expected Citations**: `[1]` Slide 8

### Q24: Khái niệm "Socket" trong mạng máy tính được định nghĩa như thế nào?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 41)
* **Ground Truth Answer**: Socket là một đối tượng đại diện cho điểm truy cập mức thấp (low-level access point) vào ngăn xếp IP (IP stack), sử dụng địa chỉ IP của node cùng giao thức mạng để tạo kênh truyền thông bảo mật và truyền dữ liệu.
* **Expected Citations**: `[1]` Slide 41

### Q25: Trong lập trình mạng .NET, namespace nào cung cấp quyền truy cập trực tiếp vào giao diện Windows Sockets (Winsock)?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 35)
* **Ground Truth Answer**: Namespace `System.Net.Sockets`.
* **Expected Citations**: `[1]` Slide 35

### Q26: Để gửi một yêu cầu GET và nhận nội dung HTML dạng chuỗi bất đồng bộ bằng `HttpClient`, ta sử dụng phương thức nào?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 26, 32)
* **Ground Truth Answer**: `client.GetStringAsync(uri)` (hoặc `await client.GetAsync(uri)` kết hợp `ReadAsStringAsync()`).
* **Expected Citations**: `[1]` Slide 26, 32

### Q27: Làm thế nào để phân biệt các tiến trình truyền thông mạng (communicating processes) khác nhau chạy trên cùng một máy tính?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 5, 10)
* **Ground Truth Answer**: Hệ điều hành phân biệt các tiến trình truyền thông mạng dựa vào số hiệu cổng (**Port number** - số nguyên không dấu 2 byte từ 0 đến 65535).
* **Expected Citations**: `[1]` Slide 5, 10

### Q28: Khi truyền dữ liệu chuỗi qua `NetworkStream`, vì sao cần chuyển đổi chuỗi thành mảng byte trước khi gửi?
* **Source Document**: `Chapter 01 - Networking Programming.pdf` (Slide 47, 49)
* **Ground Truth Answer**: Vì luồng mạng (`NetworkStream`) chỉ truyền nhận dữ liệu thô ở dạng byte (raw bytes), do đó chuỗi phải được mã hóa (ví dụ: `Encoding.ASCII.GetBytes(data)`).
* **Expected Citations**: `[1]` Slide 47, 49

---

## Category 3: Deep Architecture & Design Principles (API Theory)

### Q29: What is an Application Programming Interface (API) in computer programming?
* **Source Document**: `Application programming interface.pdf` (Page 1)
* **Ground Truth Answer**: An API is a set of routines, protocols, and tools for building software applications. It expresses a software component in terms of operations, inputs, outputs, and underlying types, defining functionalities independent of their implementations.
* **Expected Citations**: `[1]` Page 1

### Q30: How does an API differ from an Application Binary Interface (ABI)?
* **Source Document**: `Application programming interface.pdf` (Page 1)
* **Ground Truth Answer**: An API is source-code based (e.g., POSIX is an API), whereas an ABI is a binary-level interface defining machine code and data structure layouts (e.g., Linux Standard Base provides an ABI).
* **Expected Citations**: `[1]` Page 1

### Q31: Explain David Parnas' (1972) principle of "Information Hiding" and its relation to API design.
* **Source Document**: `Application programming interface.pdf` (Page 4)
* **Ground Truth Answer**: Information hiding states that software should be divided into modules with specified interfaces that hide the internal complexities and implementation details of each module, exposing only what clients need to know to use them effectively.
* **Expected Citations**: `[1]` Page 4

### Q32: What does Conway's Law suggest regarding API and software system design?
* **Source Document**: `Application programming interface.pdf` (Page 4)
* **Ground Truth Answer**: Conway's Law states that the structure of a system inevitably reflects the communication structure of the organization that created it, implying that API team structures directly influence how APIs are designed.
* **Expected Citations**: `[1]` Page 4

### Q33: How does an API operate in procedural programming languages like C? Give an example from the text.
* **Source Document**: `Application programming interface.pdf` (Page 1)
* **Ground Truth Answer**: In procedural languages, an API specifies a set of functions or routines (presented via header files and man page documentation). For example, the math API on Unix provides `sqrt()` in `<math.h>`.
* **Expected Citations**: `[1]` Page 1

### Q34: What is a "marker interface" in object-oriented APIs, and what is the standard Java example given?
* **Source Document**: `Application programming interface.pdf` (Page 2)
* **Ground Truth Answer**: A marker interface is an interface with no methods that prescribes behavior. The text cites `Serializable` in Java, which marks a class to be serialized without requiring public method implementations.
* **Expected Citations**: `[1]` Page 2

### Q35: How does a virtual machine (JVM or .NET CLR) enable API sharing and reuse across different programming languages?
* **Source Document**: `Application programming interface.pdf` (Page 3)
* **Ground Truth Answer**: Virtual machines abstract programming languages through intermediate bytecode representations, allowing languages like Groovy or Scala to natively call and share standard Java APIs.
* **Expected Citations**: `[1]` Page 3

### Q36: What was the central dispute in the Oracle America, Inc. v. Google, Inc. lawsuit regarding APIs?
* **Source Document**: `Application programming interface.pdf` (Page 7)
* **Ground Truth Answer**: The dispute was whether Java API declarations/method signatures used in the Android operating system could be copyrighted under US copyright law.
* **Expected Citations**: `[1]` Page 7

### Q37: What is a "Language Binding" and what open-source tools generate them?
* **Source Document**: `Application programming interface.pdf` (Page 7)
* **Ground Truth Answer**: A language binding is a thin mapping layer that allows an API written in one language to be naturally used in another. Tools mentioned include SWIG and F2PY.
* **Expected Citations**: `[1]` Page 7

### Q38: What architectural trend shifted Web APIs from SOAP to REST in modern web development?
* **Source Document**: `Application programming interface.pdf` (Page 3)
* **Ground Truth Answer**: The Web 2.0 trend shifted from heavyweight SOAP-based web services towards lightweight REST (Representational State Transfer) resources using HTTP, XML, and JSON.
* **Expected Citations**: `[1]` Page 3

---

## Category 4: Cross-Document Synthesis & Comparison

### Q39: Compare procedural language APIs (like C math.h) with .NET's `HttpClient` for consuming Web APIs.
* **Source Documents**: `Application programming interface.pdf` (Page 1) & `Chapter 01 - Networking Programming.pdf` (Slide 24)
* **Ground Truth Answer**: Procedural APIs in C are local library functions and header files executed in memory on a single machine, whereas `HttpClient` in .NET is a network-based protocol API designed to consume remote Web APIs and RESTful services over HTTP.
* **Expected Citations**: `[1]` API Page 1, `[2]` Slides Slide 24

### Q40: How does the concept of Web APIs (HTTP messages with JSON/XML) relate to .NET's `WebRequest` and `HttpClient` classes?
* **Source Documents**: `Application programming interface.pdf` (Page 3) & `Chapter 01 - Networking Programming.pdf` (Slide 16, 24)
* **Ground Truth Answer**: Web APIs define request/response message exchanges over HTTP. In .NET, `WebRequest` provided early protocol-agnostic handling, while `HttpClient` was specifically designed to consume modern RESTful Web APIs with JSON/XML payloads.
* **Expected Citations**: `[1]` API Page 3, `[2]` Slides Slide 24

### Q41: Compare how local API objects communicate in memory versus remote object communication via protocols like CORBA/RMI or TCP.
* **Source Documents**: `Application programming interface.pdf` (Page 3) & `Chapter 01 - Networking Programming.pdf` (Slide 36)
* **Ground Truth Answer**: Local APIs exchange object references directly in process memory. Remote communication (CORBA/RMI/TCP) marshals object data into byte streams and transmits them over a network channel established between IP endpoints.
* **Expected Citations**: `[1]` API Page 3, `[2]` Slides Slide 36

### Q42: What role does documentation play in API usability according to both the API specification document and the .NET namespaces overview?
* **Source Documents**: `Application programming interface.pdf` (Page 2, 5) & `Chapter 01 - Networking Programming.pdf` (Slide 16)
* **Ground Truth Answer**: API documentation (e.g., JavaDoc, .NET XML docs, man pages) is essential because without explicit descriptions of signatures, methods, exceptions, and protocols, the API cannot be correctly consumed.
* **Expected Citations**: `[1]` API Page 2, 5

### Q43: How do inversion of control in frameworks and event-driven sockets differ from standard API calling?
* **Source Documents**: `Application programming interface.pdf` (Page 2) & `Chapter 01 - Networking Programming.pdf` (Slide 40, 48)
* **Ground Truth Answer**: In standard APIs, the caller controls the execution flow. In frameworks and asynchronous sockets (`BeginConnect`, event handlers), inversion of control passes execution management to the framework or runtime callbacks.
* **Expected Citations**: `[1]` API Page 2, `[2]` Slides Slide 40

### Q44: Compare the deprecation and stability of public APIs with version changes from .NET Framework to .NET Core for `HttpClient`.
* **Source Documents**: `Application programming interface.pdf` (Page 5) & `Chapter 01 - Networking Programming.pdf` (Slide 24)
* **Ground Truth Answer**: Public APIs must document unstable features (e.g. `@Beta`) and deprecation policies. In .NET Framework, `HttpClient` originally relied on `WebRequest`, but in .NET Core it was re-architected to handle HTTP directly, modernizing the API implementation while preserving interface compatibility.
* **Expected Citations**: `[1]` API Page 5, `[2]` Slides Slide 24

---

## Category 5: Strict Anti-Hallucination & Guardrail Refusal Tests

### Q45: How do I configure Entity Framework Core to use Pomelo MySQL with connection pooling in ASP.NET Core?
* **Target Behavior**: Refuse to answer (Out-of-Scope).
* **Expected Response**: State that the provided materials do not contain information regarding Pomelo MySQL or EF Core connection pooling, and refuse to speculate.
* **Expected Citations**: None (Strict Grounding Guardrail).

### Q46: Giải thích giải thuật Dijkstra tìm đường đi ngắn nhất trong đồ thị có hướng?
* **Target Behavior**: Refuse to answer (Out-of-Scope).
* **Expected Response**: Thông báo tài liệu môn học được chọn không đề cập đến giải thuật Dijkstra tìm đường đi ngắn nhất.
* **Expected Citations**: None (Strict Grounding Guardrail).

### Q47: What are the best practices for setting up a Kubernetes cluster with Helm charts on AWS EKS?
* **Target Behavior**: Refuse to answer (Out-of-Scope).
* **Expected Response**: State that Kubernetes, Helm, and AWS EKS are not covered in the selected networking and API documents.
* **Expected Citations**: None (Strict Grounding Guardrail).

### Q48: Làm thế nào để tạo một React hook tùy chỉnh (custom hook) để quản lý Redux state?
* **Target Behavior**: Refuse to answer (Out-of-Scope).
* **Expected Response**: Thông báo từ chối do tài liệu không chứa nội dung về React, Redux, hay frontend hooks.
* **Expected Citations**: None (Strict Grounding Guardrail).

### Q49: Explain the difference between MongoDB replica sets and sharded clusters.
* **Target Behavior**: Refuse to answer (Out-of-Scope).
* **Expected Response**: State that MongoDB NoSQL clustering is not discussed in the provided documents.
* **Expected Citations**: None (Strict Grounding Guardrail).

### Q50: How do I implement an OAuth2 Authorization Code flow with PKCE in Flutter?
* **Target Behavior**: Refuse to answer (Out-of-Scope).
* **Expected Response**: State that Flutter and OAuth2 PKCE implementation details are not present in the current document scope.
* **Expected Citations**: None (Strict Grounding Guardrail).
