# 프로젝트 규칙

## git push 단축 명령

사용자가 "git push"라고 하면 아래 순서를 자동으로 실행한다.

1. `git status` + `git diff` 로 변경사항 확인
2. 변경사항을 작업 단위로 묶어 여러 커밋으로 분리
   - 예) 스크립트 변경 / 씬 변경 / ProjectSettings 변경은 별도 커밋
   - 논리적으로 연관된 파일끼리 묶어 커밋
3. 각 단위별로 `git add` → `git commit` 반복
4. 전체 커밋 완료 후 `git push`

별도 확인 없이 바로 진행한다.

## 플레이어 MVP 구현 방침

### 설계 원칙
- Rigidbody에만 의존하지 않고 velocity를 직접 제어 (정밀 플랫포머)
- 책임을 컴포넌트로 분리 (모듈화)
  - **Input** : 키 입력 수집
  - **Mover** : velocity 계산 및 이동 적용
  - **Sensor** : 지면/벽 감지 (CollisionSensor)
  - **State** : 현재 상태 관리 (Grounded, Jumping, Falling …)

### 구현 순서
1. 이동 (좌우 이동 + 가감속)
2. 점프 (기본 점프)
3. 손맛 보정 4종 — 초반에 반드시 포함
   - 가감속 (Acceleration / Deceleration)
   - 가변 점프 (Variable Jump Height — 버튼을 짧게 누르면 낮게)
   - 코요테 타임 (Coyote Time — 낙하 직후 짧은 시간 점프 허용)
   - 점프 버퍼 (Jump Buffer — 착지 직전 입력 선입력 허용)
