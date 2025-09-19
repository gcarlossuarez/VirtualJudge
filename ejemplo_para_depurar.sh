curl -v -X POST http://localhost:8080/compile-run \
  -F "studentId=333" \
  -F "problemId=5001" \
  -F "code=@program.cs" \
  -F "language=csharp"
