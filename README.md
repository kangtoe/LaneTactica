# LaneTactica

PVZ 스타일의 레인 기반 타워 디펜스 게임

## 게임 개요

- **장르**: 레인 기반 타워 디펜스
- **플랫폼**: PC (Unity)
- **타겟**: 캐주얼 전략 게임 유저
- **레퍼런스**: Plants vs Zombies

## 핵심 게임플레이

1. 자원(에너지)을 모아 유닛 배치
2. 각 레인에서 적의 진격 저지
3. 웨이브를 모두 클리어하면 승리

## 기술 스택

- **엔진**: Unity 6.0+
- **언어**: C#
- **아키텍처**: Component 기반 + ScriptableObject 데이터

## 문서

- [게임플레이](docs/Gameplay.md) - 기본 구조, 자원 시스템, 게임 흐름, 핵심 시스템
- [유닛 공통](docs/UnitBase.md) - 타워와 적의 공통 요소/동작
- [아군 유닛 (타워)](docs/Towers.md) - 타워 전용 요소/동작, 유닛 목록
- [적 유닛](docs/Enemies.md) - 적 전용 요소/동작, 유닛 목록
- [적 스폰](docs/EnemySpawn.md) - 웨이브 및 스폰 시스템
- [로드맵](docs/Roadmap.md) - 기능별 개발 순서와 중요도
